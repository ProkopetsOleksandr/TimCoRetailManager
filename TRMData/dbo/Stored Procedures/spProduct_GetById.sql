CREATE PROCEDURE [dbo].[spProduct_GetById]
	@Id int = 0
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Id, ProductName, [Description], RetailPrice, QuantityInStock, IsTaxable
	FROM dbo.Product
	WHERE Id = @Id
END
