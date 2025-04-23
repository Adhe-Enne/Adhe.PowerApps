function loadBibliotecas() {
  const bibliotecas = document.getElementById("biblioteca");
  bibliotecas.innerHTML = '<option value="">Cargando...</option>';

  window.parent.Xrm.WebApi.retrieveMultipleRecords("dn_biblioteca", "?$select=dn_bibliotecaid,dn_name")
    .then(function (result) {
      bibliotecas.innerHTML = '<option value="">Seleccione una biblioteca</option>';
      result.entities.forEach((bib) => {
        let option = document.createElement("option");
        option.value = bib.dn_bibliotecaid;
        option.innerHTML = bib.dn_name;
        bibliotecas.appendChild(option);
      });
    })
    .catch(function (error) {
      console.error("Error cargando bibliotecas:", error);
    });
}

function loadLibros() {
  let libroSelect = document.getElementById("libro");
  libroSelect.innerHTML = '<option value="">Seleccione un libro</option>';
  libroSelect.disabled = true;

  var bibSelect = document.getElementById("biblioteca");
  var bibliotecaId = bibSelect.options[bibSelect.selectedIndex].value;

  if (!bibliotecaId) return;

  window.parent.Xrm.WebApi.retrieveMultipleRecords(
    "dn_bibliotecalibro",
    `?$select=dn_bibliotecalibroid,_dn_libro_value,dn_unidades&$filter=(_dn_biblioteca_value eq ${bibliotecaId}  and dn_unidades gt 0) `
  )
    .then(function (response) {
      response.entities.forEach((lib) => {
        let option = document.createElement("option");
        option.value = lib._dn_libro_value;
        option.textContent = lib["_dn_libro_value@OData.Community.Display.V1.FormattedValue"];

        libroSelect.appendChild(option);
        libroSelect.disabled = false;
      });
    })
    .catch(function (error) {
      console.error("Error al cargar libros:", error.message);
    });
}

function enviarAlquiler() {
  debugger;
  window.parent.Xrm.Utility.showProgressIndicator("Cargando alquileres...");

  const socioId = window.parent.Xrm.Page.data.entity.getId();
  const bibliotecaId = document.getElementById("biblioteca").value;
  const libroId = document.getElementById("libro").value;
  const fechaDesde = document.getElementById("fechaDesde").value;
  const fechaHasta = document.getElementById("fechaHasta").value;

  if (!bibliotecaId || !libroId || !fechaDesde || !fechaHasta) {
    alert("Por favor, complete todos los campos.");
    return;
  }

  var record = {};
  record["dn_Biblioteca@odata.bind"] = "/dn_bibliotecas(" + bibliotecaId + ")";
  record["dn_Libro@odata.bind"] = "/dn_libros(" + libroId + ")";
  record["dn_Socio@odata.bind"] = "/dn_socios(" + socioId.replace(/[{}]/g, "") + ")";
  record.dn_desde = fechaDesde; // Date Time
  record.dn_hasta = fechaHasta; // Date Time

  window.parent.Xrm.WebApi.createRecord("dn_alquiler", record)
    .then(function success(response) {
      if (response.ok) {
        alert("Alquiler registrado con éxito.");
        window.parent.Xrm.Page.data.refresh();
      }
    })
    .catch(function (error) {
      console.error("Error registrando alquiler:", error);
    })
    .finally(() => {
      window.parent.Xrm.Utility.closeProgressIndicator();
    });
}

document.addEventListener("DOMContentLoaded", function () {
  loadBibliotecas();
});
