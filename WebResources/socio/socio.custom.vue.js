function cargarAlquileres() {
  if (!window.parent || !window.parent.Xrm) {
    console.error("No se pudo acceder a Xrm.");
    return;
  }

  var globalContext = window.parent.Xrm;
  var formContext = globalContext.Page ? globalContext.Page : null;

  if (!formContext || !formContext.data) {
    console.error("No se pudo obtener el contexto del formulario.");
    return;
  }

  if (formContext.ui && formContext.ui.getFormType() === 1) return;

  var socioId = formContext.data.entity.getId();
  if (!socioId) {
    console.error("SocioId Inválido");
    return;
  }

  socioId = socioId.replace("{", "").replace("}", "");

  globalContext.Utility.showProgressIndicator("Cargando alquileres...");

  globalContext.WebApi.retrieveMultipleRecords(
    "dn_alquiler",
    "?$select=dn_alquilerid,_dn_biblioteca_value,dn_desde,_dn_libro_value&$filter=_dn_socio_value eq " +
      socioId
  )
    .then(function (result) {
      // Llenar los datos en Vue
      app.alquileres = result.entities.map((alquiler) => ({
        biblioteca:
          alquiler[
            "_dn_biblioteca_value@OData.Community.Display.V1.FormattedValue"
          ] || "N/A",
        libro:
          alquiler[
            "_dn_libro_value@OData.Community.Display.V1.FormattedValue"
          ] || "N/A",
        desde: alquiler["dn_desde"] || "Sin fecha",
      }));
    })
    .catch(function (error) {
      console.error("Error al obtener datos:", error.message);
    })
    .finally(() => {
      globalContext.Utility.closeProgressIndicator();
    });
}

document.addEventListener("DOMContentLoaded", function () {
  cargarAlquileres();
});
