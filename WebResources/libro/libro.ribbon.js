function CallCustomApi(formContext) {
  debugger;

  var nombre = formContext.getAttribute("dn_nombre").getValue();

  if (!nombre) {
    Xrm.Utility.alertDialog("⚠️ Error: El campo 'Nombre' está vacío.");
    Xrm.Utility.closeProgressIndicator();
    return;
  }

  Xrm.Utility.showProgressIndicator("Procesando...");

  var request = {
    NombreIn: nombre, // Enviar el parámetro requerido
    getMetadata: function () {
      return {
        boundParameter: null, // API no está vinculada a una entidad
        parameterTypes: {
          NombreIn: { typeName: "Edm.String", structuralProperty: 1 }, // Tipo de dato string
        },
        operationType: 0, // 0 = Acción
        operationName: "dn_LibroConsumer", // Nombre del Custom API
      };
    },
  };

  // Llamar a la API
  Xrm.WebApi.online
    .execute(request)
    .then(function (response) {
      if (response.ok) {
        return response.json();
      } else {
        throw new Error("❌ Error en la llamada a la API");
      }
    })
    .then(function (data) {
      console.log("✅ Respuesta API:", data);

      // Procesar la respuesta
      var mensaje = `✅ Éxito: ${data.ConsumerSuccess}\n📩 Mensaje: ${data.ConsumerMessage}\n📄 Contenido: ${data.ConsumerContent}`;
      Xrm.Utility.alertDialog(mensaje);
    })
    .catch(function (error) {
      console.error("⚠️ Error en Custom API:", error);
      Xrm.Utility.alertDialog("❌ Error: " + error.message);
    })
    .finally(() => {
      Xrm.Utility.closeProgressIndicator();
    });
}

async function obtenerPersonajesMarvel() {
  debugger;
  try {
    const _URL = "https://gateway.marvel.com/v1/public/characters";
    const _TS = await obtenerVariableEntorno("MARVEL_TIMESTAMP"); // Timestamp actual
    const _ApiKey = await obtenerVariableEntorno("MARVEL_APIKEY_PUBLIC"); // Reemplaza con tu clave pública
    const _PrivateKey = await obtenerVariableEntorno("MARVEL_APIKEY_PRIVATE"); // Reemplaza con tu clave privada
    const name = "hulk";
    // Generar el hash MD5: md5(ts + privateKey + publicKey)
    const _Hash = await obtenerVariableEntorno("MARVEL_HASH_CONST");

    // Construir la URL con los parámetros de autenticación
    const url = `${_URL}?ts=${_TS}&apikey=${_ApiKey}&hash=${_Hash}&nameStartsWith=${name}`;

    const response = await fetch(url);
    if (!response.ok) {
      throw new Error(`Error HTTP: ${response.status}`);
    }

    const data = response.json();
    console.log("✅ Respuesta de Marvel API:", data);

    // Mostrar nombres de los personajes en la consola
    data.data.results.forEach((personaje) => {
      console.log(`🦸‍♂️ Personaje: ${personaje.description}`);
    });
  } catch (error) {
    console.error("❌ Error al obtener los datos:", error);
  }
}

function obtenerVariableEntorno(nombreVariable) {
  return new Promise((resolve, reject) => {
    Xrm.WebApi.retrieveMultipleRecords(
      "environmentvariabledefinition",
      `?$select=defaultvalue&$filter=displayname eq '${nombreVariable}' `
    )
      .then((result) => {
        if (
          result.entities.length > 0 &&
          result.entities[0].defaultvalue.length > 0
        ) {
          resolve(result.entities[0].defaultvalue);
        } else {
          console.warn(
            `No se encontró la variable de entorno: ${nombreVariable}`
          );
          resolve(null);
        }
      })
      .catch((error) => {
        console.error(
          `Error al recuperar la variable de entorno: ${nombreVariable}`,
          error.message
        );
        reject(error);
      });
  });
}

async function obtenerCredencialesMarvel() {
  try {
    const apiKeyPublic = await obtenerVariableEntorno("MARVEL_APIKEY_PUBLIC");
    const apiKeyPrivate = await obtenerVariableEntorno("MARVEL_APIKEY_PRIVATE");
    const hash = await obtenerVariableEntorno("MARVEL_HASH_CONST");
    const timestamp = await obtenerVariableEntorno("MARVEL_TIMESTAMP");

    console.log("API Key Pública:", apiKeyPublic);
    console.log("API Key Privada:", apiKeyPrivate);
    console.log("Hash:", hash);
    console.log("Timestamp:", timestamp);
  } catch (error) {
    console.error("Error al obtener las credenciales:", error);
  }
}
