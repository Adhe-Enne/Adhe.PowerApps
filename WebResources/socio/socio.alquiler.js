let bibliotecasLibros = [];
let formContext = window.parent.Xrm.Page || window.parent.Xrm.Utility.getPageContext();
let xrm = window.parent.Xrm;
let socioExterno = null;
function checkSocioExistence() {
  let dniAttr = formContext.getAttribute("dn_dni");

  if (!dniAttr || !dniAttr.getValue()) {
    document.getElementById("status").innerText = "DNI no encontrado.";
    return;
  }

  let dni = String(dniAttr.getValue());
  window.parent.Xrm.Utility.showProgressIndicator("Procesando...");

  let request = {
    DniSocioIn: dni,
    getMetadata: function () {
      return {
        boundParameter: null,
        parameterTypes: {
          DniSocioIn: { typeName: "Edm.String", structuralProperty: 1 },
        },
        operationType: 0,
        operationName: "dn_SocioGetByDniExterno",
      };
    },
  };

  window.parent.Xrm.WebApi.online
    .execute(request)
    .then(function success(response) {
      if (response.ok) return response.json();
    })
    .then(async function (result) {
      let apiResult = validateApiRestul(result.SocioOut);

      if (apiResult.data.total === 1 && apiResult.data.result[0].dni === dni) {
        socioExterno = apiResult.data.result[0];
        document.getElementById("accionesSocio").style.display = "block"; // Muestra los botones
        document.getElementById("status").innerHTML = "<span class='exists'>Este socio existe en el otro entorno.</span>";
      } else {
        document.getElementById("status").innerHTML = "<span class='not-exists'>Este socio no existe en el otro entorno.</span>";
        document.getElementById("createButton").style.display = "block";
        //OpenDialog(`El Socio no existe en el entorno externo`, "Atencion");
        console.log(result);
      }
    })
    .catch(function (error) {
      console.error(error);
      document.getElementById("status").innerText = "Error al consultar el socio.";
    })
    .finally(() => {
      window.parent.Xrm.Utility.closeProgressIndicator(); // ✅ Se ejecuta cuando todo termina
    });
}

function createSocio() {
  window.parent.Xrm.Utility.showProgressIndicator("Procesando...");

  let formContext = window.parent.Xrm.Page || window.parent.Xrm.Utility.getPageContext();

  // Capturar los valores de los campos
  let nombre = formContext.getAttribute("dn_nombre")?.getValue() || null;
  let apellido = formContext.getAttribute("dn_apellido")?.getValue() || null;
  let dni = formContext.getAttribute("dn_dni")?.getValue() || null;
  let tipoTelefono = formContext.getAttribute("dn_tipoTelefono")?.getValue() || null;
  let telefono = formContext.getAttribute("dn_telefono")?.getValue() || null;

  if (!dni) {
    document.getElementById("status").innerText = "No se encontró el DNI.";
    return;
  }

  // Construir el JSON en formato string
  let body = JSON.stringify({
    nombre: nombre,
    apellido: apellido,
    dni: String(dni), // Convertir explícitamente a string
    tipoTelefono: tipoTelefono,
    telefono: telefono,
  });

  let request = {
    SocioBodyIn: body, // El API espera un string con el JSON
    getMetadata: function () {
      return {
        boundParameter: null,
        parameterTypes: {
          SocioBodyIn: { typeName: "Edm.String", structuralProperty: 1 },
        },
        operationType: 0,
        operationName: "dn_CreateSocioExternalEnviroment",
      };
    },
  };

  window.parent.Xrm.WebApi.online
    .execute(request)
    .then(function success(response) {
      if (response.ok) return response.json();
    })
    .then(function (apiResult) {
      console.log(apiResult);
      let result = validateApiRestul(apiResult.ResponseMessage);
      document.getElementById("status").innerText = result.message;
      document.getElementById("createButton").style.display = "none";
      document.getElementById("accionesSocio").style.display = "block"; // Muestra los botones
    })
    .catch(function (error) {
      console.error(error);
      OpenDialog(`Error al Crear el socio `, "ERROR");
    })
    .finally(() => {
      window.parent.Xrm.Utility.closeProgressIndicator(); // ✅ Se ejecuta cuando todo termina
    });
}

async function mostrarFormulario() {
  document.getElementById("grillaAlquileres").style.display = "none";
  document.getElementById("alquiler").style.display = "block";
  await consultarBibliotecas(); // Esperamos que termine antes de continuar
  await cargarBibliotecas();
}

async function consultarBibliotecas() {
  window.parent.Xrm.Utility.showProgressIndicator("Cargando Formulario...");

  let request = {
    getMetadata: function () {
      return {
        boundParameter: null,
        operationType: 0,
        operationName: "dn_GetBibliotecas",
      };
    },
  };

  try {
    let response = await window.parent.Xrm.WebApi.online.execute(request);
    if (response.ok) {
      let result = await response.json();
      let apiResult = validateApiRestul(result.BibliotecasOut);
      bibliotecasLibros = apiResult.data.result; // Almacenamos el resultado correctamente
    }
  } catch (error) {
    console.error(error);
    OpenDialog(`Error al cargar Bibliotecas`, "ERROR");
  } finally {
    window.parent.Xrm.Utility.closeProgressIndicator();
  }
}

async function cargarBibliotecas() {
  let selectBiblioteca = document.getElementById("biblioteca");
  selectBiblioteca.innerHTML = `<option value="">Seleccione una biblioteca</option>`; // Limpia el select

  let bibliotecasUnicas = new Set(); // Usamos un Set para almacenar valores únicos

  bibliotecasLibros.forEach((b) => {
    if (!bibliotecasUnicas.has(b.bibliotecaGuid)) {
      bibliotecasUnicas.add(b.bibliotecaGuid); // Agregamos el GUID al Set

      let option = document.createElement("option");
      option.value = b.bibliotecaGuid; // Usamos el GUID como valor único
      option.textContent = b.biblioteca; // Nombre de la biblioteca
      selectBiblioteca.appendChild(option);
    }
  });
}

function loadLibros() {
  let selectBiblioteca = document.getElementById("biblioteca");
  let selectLibros = document.getElementById("libro");

  let bibliotecaSeleccionada = selectBiblioteca.value;
  selectLibros.innerHTML = `<option value="">Seleccione un libro</option>`;

  if (!bibliotecaSeleccionada) {
    selectLibros.innerHTML = `<option value="">No se encontraron libros en esta biblioteca</option>`;
    return;
  }

  let librosFiltrados = bibliotecasLibros.filter((b) => b.bibliotecaGuid === bibliotecaSeleccionada);

  librosFiltrados.forEach((libro) => {
    let option = document.createElement("option");
    option.value = libro.libroGuid; // ID único del libro
    option.textContent = libro.libro; // Nombre del libro
    selectLibros.appendChild(option);
  });

  selectLibros.disabled = false;
}

function enviarAlquiler() {
  debugger;
  xrm.Utility.showProgressIndicator("Enviando alquiler al dataverse externo...");

  let biblioteca = document.getElementById("biblioteca").value;
  let libro = document.getElementById("libro").value;
  let fechaDesde = document.getElementById("fechaDesde").value;
  let fechaHasta = document.getElementById("fechaHasta").value;

  if (!biblioteca || !libro || !fechaDesde || !fechaHasta) {
    alert("Por favor, complete todos los campos.");
    return;
  }

  let dniAttr = formContext.getAttribute("dn_dni");

  if (!dniAttr || !dniAttr.getValue()) {
    document.getElementById("status").innerText = "DNI no encontrado.";
    return;
  }

  let dni = String(dniAttr.getValue());

  if (socioExterno === null || socioExterno.SocioId === null) {
    OpenDialog("No se encontro informacion del Socio en entorno local: SocioId");
    return;
  }
  debugger;

  let body = JSON.stringify({
    Socio: socioExterno.id,
    Libro: libro,
    Biblioteca: biblioteca,
    Desde: fechaDesde,
    Hasta: fechaHasta,
  });

  var request = {
    // Parameters
    AlquilerIn: body, // Edm.String
    getMetadata: function () {
      return {
        boundParameter: null,
        parameterTypes: {
          AlquilerIn: { typeName: "Edm.String", structuralProperty: 1 },
        },
        operationType: 0,
        operationName: "dn_CreateAlquilerExternalEnvironment",
      };
    },
  };

  window.parent.Xrm.WebApi.online
    .execute(request)
    .then(function success(response) {
      if (response.ok) return response.json();
    })
    .then(function (apiResult) {
      console.log(apiResult);
      let result = validateApiRestul(apiResult.ResponseMessage);
      OpenDialog(result.message, "Operacion Exitosa!");
    })
    .catch(function (error) {
      console.error(error);
      OpenDialog(`Error al Crear Alquiler `, "ERROR");
    })
    .finally(() => {
      console.log("Datos enviados:", { biblioteca, libro, fechaDesde, fechaHasta });
      document.getElementById("alquiler").style.display = "none";

      xrm.Utility.closeProgressIndicator(); // ✅ Se ejecuta cuando todo termina
    });
}

function mostrarAlquileres() {
  window.parent.Xrm.Utility.showProgressIndicator("Cargando Alquileres...");
  debugger;
  var request = {
    // Parameters
    GuidSocioIn: socioExterno.id, // Edm.String

    getMetadata: function () {
      return {
        boundParameter: null,
        parameterTypes: {
          GuidSocioIn: { typeName: "Edm.String", structuralProperty: 1 },
        },
        operationType: 0,
        operationName: "dn_SocioGetAlquilerByDni",
      };
    },
  };

  window.parent.Xrm.WebApi.online
    .execute(request)
    .then(function success(response) {
      if (response.ok) return response.json();
    })
    .then(function (apiResult) {
      console.log(apiResult);
      let result = validateApiRestul(apiResult.AlquileresOut);
      llenarTabla(result.data.result);
    })
    .catch(function (error) {
      console.error(error);
      OpenDialog(`Error al Consultar alquileres `, "ERROR");
    })
    .finally(() => {
      window.parent.Xrm.Utility.closeProgressIndicator(); // ✅ Se ejecuta cuando todo termina
    });
}

function llenarTabla(alquileres) {
  // Limpiamos la tabla antes de llenarla
  let tabla = document.getElementById("tablaAlquileres");
  tabla.innerHTML = "";

  if (alquileres && alquileres.length > 0) {
    alquileres.forEach((alquiler) => {
      let fila = document.createElement("tr");

      fila.innerHTML = `
                        <td>${alquiler.biblioteca}</td>
                        <td>${alquiler.libro}</td>
                        <td>${formatDate(alquiler.desde)}</td>
                        <td>${formatDate(alquiler.hasta)}</td>
                    `;

      tabla.appendChild(fila);
    });

    // Mostramos la tabla de alquileres
    document.getElementById("grillaAlquileres").style.display = "block";
  } else {
    // Si no hay alquileres, mostramos un mensaje en la tabla
    let fila = document.createElement("tr");
    fila.innerHTML = `<td colspan="4" style="text-align:center;">No hay alquileres registrados</td>`;
    tabla.appendChild(fila);

    // También mostramos la tabla para que se vea el mensaje
    document.getElementById("grillaAlquileres").style.display = "block";
  }
}
// Ejecutar la validación al cargar la página
document.addEventListener("DOMContentLoaded", function () {
  checkSocioExistence();
  document.getElementById("createButton").addEventListener("click", createSocio);
});
