function cargarAlquileres(executionContext) {
  var formContext = executionContext.getFormContext();
  if (formContext.ui.getFormType() === 1) return;

  var socioId = formContext.data.entity
    .getId()
    .toString()
    .replace("{", "")
    .replace("}", ""); // ID del socio

  if (!socioId) {
    console.log("SocioId Invalido");
    return;
  }
  Xrm.Utility.showProgressIndicator("Procesando...");

  Xrm.WebApi.retrieveMultipleRecords(
    "dn_alquiler",
    "?$select=dn_alquilerid,_dn_biblioteca_value,dn_desde,_dn_libro_value&$filter=_dn_socio_value eq " +
      socioId
  )
    .then(function (result) {
      debugger;
      var tbody = document.querySelector("#tablaAlquileres tbody");
      tbody.innerHTML = ""; // Limpiar la tabla antes de llenarla

      result.entities.forEach(function (alquiler) {
        var row = tbody.insertRow();
        row.insertCell(0).innerText =
          alquiler[
            "_dn_biblioteca_value@OData.Community.Display.V1.FormattedValue"
          ];
        row.insertCell(1).innerText =
          alquiler["_dn_libro_value@OData.Community.Display.V1.FormattedValue"];
        row.insertCell(2).innerText = alquiler["dn_desde"];
      });
    })
    .catch(function (error) {
      debugger;
      console.log(error.message);
    })
    .finally(() => {
      Xrm.Utility.closeProgressIndicator(); // ✅ Se ejecuta cuando todo termina
    });
}

document.addEventListener("DOMContentLoaded", function () {
  cargarAlquileres();
});
