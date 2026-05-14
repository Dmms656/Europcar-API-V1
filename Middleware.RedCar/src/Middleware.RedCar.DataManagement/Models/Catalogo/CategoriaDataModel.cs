namespace Middleware.RedCar.DataManagement.Models.Catalogo;

public sealed record CategoriaDataModel(
    int IdCategoria,
    string Codigo,
    string Nombre,
    string Descripcion,
    string Estado);
