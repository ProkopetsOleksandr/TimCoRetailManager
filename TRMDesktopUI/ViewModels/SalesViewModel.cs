using AutoMapper;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TRMDesktopUI.Library.Api;
using TRMDesktopUI.Library.Helpers;
using TRMDesktopUI.Library.Models;
using TRMDesktopUI.Models;

namespace TRMDesktopUI.ViewModels
{
    public class SalesViewModel : Screen
    {
        private IProductEndpoint _productEndpoint;
        private ISaleEndpoint _saleEndpoint;
        private IMapper _mapper;
        private IWindowManager _windowManager;
        private IConfigHelper _configHelper { get; }

        private BindingList<ProductDisplayModel> _products;
        private BindingList<CartItemDisplayModel> _cart;
        private int _itemQuantity = 1;
        private ProductDisplayModel _selectedProduct;
        private CartItemDisplayModel _selectedCartItem;

        public SalesViewModel(
            IProductEndpoint productEndpoint, 
            IConfigHelper configHelper, 
            ISaleEndpoint saleEndpoint, 
            IMapper mapper,
            IWindowManager windowManager)
        {
            _productEndpoint = productEndpoint;
            _saleEndpoint = saleEndpoint;
            _configHelper = configHelper;
            _mapper = mapper;
            _windowManager = windowManager;
            _cart = new BindingList<CartItemDisplayModel>();
        }

        public ProductDisplayModel SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                NotifyOfPropertyChange(() => SelectedProduct);
                NotifyOfPropertyChange(() => CanAddToCart);
            }
        }

        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            try
            {
                await LoadProducts();
            }
            catch (Exception)
            {
                var status = IoC.Get<StatusInfoViewModel>();
                status.Update("Unauthorize access", "Sorry, you don't have permission to see this page");

                dynamic settings = new ExpandoObject();
                settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                settings.ResizeMode = ResizeMode.NoResize;
                settings.Title = "System Erorr";

                await _windowManager.ShowDialogAsync(status, null, settings);
                await TryCloseAsync();
            }
        }

        public CartItemDisplayModel SelectedCartItem
        {
            get { return _selectedCartItem; }
            set
            {
                _selectedCartItem = value;
                NotifyOfPropertyChange(() => SelectedCartItem);
                NotifyOfPropertyChange(() => CanRemoveFromCart);
            }
        }

        public BindingList<ProductDisplayModel> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                NotifyOfPropertyChange(() => Products);
            }
        }

        public BindingList<CartItemDisplayModel> Cart
        {
            get { return _cart; }
            set
            {
                _cart = value;
                NotifyOfPropertyChange(() => Cart);
            }
        }

        public int ItemQuantity
        {
            get { return _itemQuantity; }
            set
            {
                _itemQuantity = value;
                NotifyOfPropertyChange(() => ItemQuantity);
                NotifyOfPropertyChange(() => CanAddToCart);
            }
        }

        public string SubTotal => CalculateSubTotal().ToString("c");
        public string Tax => CalculateTax().ToString("c");
        public string Total => (CalculateSubTotal() + CalculateTax()).ToString("c");

        public bool CanAddToCart
        {
            get
            {
                if (ItemQuantity > 0 && SelectedProduct?.QuantityInStock >= ItemQuantity)
                {
                    return true;
                }

                return false;
            }
        }

        public void AddToCart()
        {
            var existingItem = Cart.FirstOrDefault(m => m.Product == SelectedProduct);
            if (existingItem != null)
            {
                existingItem.QuantityInCart += ItemQuantity;
            }
            else
            {
                var item = new CartItemDisplayModel()
                {
                    Product = SelectedProduct,
                    QuantityInCart = ItemQuantity
                };

                Cart.Add(item);
            }

            SelectedProduct.QuantityInStock -= ItemQuantity;
            ItemQuantity = 1;

            NotifyOfPropertyChange(() => SubTotal);
            NotifyOfPropertyChange(() => Tax);
            NotifyOfPropertyChange(() => Total);
            NotifyOfPropertyChange(() => CanCheckOut);
        }

        public bool CanRemoveFromCart => SelectedCartItem != null;

        public void RemoveFromCart()
        {
            SelectedCartItem.Product.QuantityInStock++;

            if (SelectedCartItem.QuantityInCart > 1)
            {
                SelectedCartItem.QuantityInCart--;
            } 
            else
            {
                Cart.Remove(SelectedCartItem);
            }

            NotifyOfPropertyChange(() => SubTotal);
            NotifyOfPropertyChange(() => Tax);
            NotifyOfPropertyChange(() => Total);
            NotifyOfPropertyChange(() => CanCheckOut);
            NotifyOfPropertyChange(() => CanAddToCart);
        }

        public bool CanCheckOut => Cart.Count > 0;

        public async Task CheckOut()
        {
            var sale = new SaleModel()
            {
                SaleDetails = new List<SaleDetailModel>()
            };

            foreach(var item in Cart)
            {
                sale.SaleDetails.Add(new SaleDetailModel
                {
                    ProductId = item.Product.Id,
                    Quantity = item.QuantityInCart
                });
            }

            await _saleEndpoint.PostSale(sale);
            await ResetSalesViewModel();
        }

        private decimal CalculateSubTotal()
        {
            return Cart.Sum(m => m.Product.RetailPrice * m.QuantityInCart);
        }

        private decimal CalculateTax()
        {
            var taxRate = _configHelper.GetTaxRate() / 100;

            return Cart
                .Where(m => m.Product.IsTaxable)
                .Sum(m => m.Product.RetailPrice * m.QuantityInCart * taxRate);
        }

        private async Task ResetSalesViewModel()
        {
            _cart.Clear();
            _selectedCartItem = null;
            NotifyOfPropertyChange(() => SubTotal);
            NotifyOfPropertyChange(() => Tax);
            NotifyOfPropertyChange(() => Total);
            NotifyOfPropertyChange(() => CanCheckOut);
            NotifyOfPropertyChange(() => CanAddToCart);
            NotifyOfPropertyChange(() => CanRemoveFromCart);

            await LoadProducts();
        }

        private async Task LoadProducts()
        {
            var products = await _productEndpoint.GetAll();
            Products = _mapper.Map<BindingList<ProductDisplayModel>>(products);
        }
    }
}
