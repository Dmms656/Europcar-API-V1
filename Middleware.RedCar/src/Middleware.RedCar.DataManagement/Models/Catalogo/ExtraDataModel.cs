namespace Middleware.RedCar.DataManagement.Models.Catalogo;

public sealed record ExtraDataModel(
    int IdExtra,
    string Codigo,
    string Nombre,
    string Descripcion,
    decimal ValorFijo,
    string Estado);
