function AppViewModel() {
  this.titulo = "Knockout Alquileres";
  this.bibliotecaHeader = "Biblioteca";
  this.libroHeader = "Libro";
  this.fechaDesdeHeader = "Fecha Alquiler";
  // Arreglo observable para almacenar los alquileres
  this.alquileres = ko.observableArray([]);

  // Función para cargar los alquileres
  this.cargarAlquileres = function () {
    debugger;
    let queryString = frames.parent.location.href;
    let urlParams = new URLSearchParams(queryString);
    let socioGuid = urlParams.get("id");

    window.parent.Xrm.WebApi.retrieveMultipleRecords(
      "dn_alquiler",
      `?$select=_dn_biblioteca_value,dn_desde,_dn_libro_value,_dn_socio_value&$filter=_dn_socio_value eq ${socioGuid}`
    ).then(
      function success(results) {
        debugger;
        for (var i = 0; i < results.entities.length; i++) {
          var result = results.entities[i];
          var biblioteca =
            result[
              "_dn_biblioteca_value@OData.Community.Display.V1.FormattedValue"
            ];
          var libro =
            result["_dn_libro_value@OData.Community.Display.V1.FormattedValue"];
          var fechaDesde = result["dn_desde"];

          // Agregar el registro al arreglo observable
          viewModel.alquileres.push({
            biblioteca: biblioteca,
            libro: libro,
            fechaDesde: fechaDesde,
          });
        }
        console.log(viewModel.alquileres);
      },
      function (error) {
        debugger;
        console.log(error.message);
      }
    );
  };
  // Cargar los alquileres al iniciar la página
  this.cargarAlquileres();
}

var viewModel = new AppViewModel();
ko.applyBindings(viewModel);
