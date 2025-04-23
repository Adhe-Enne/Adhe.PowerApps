//consumo de api por middleware
//CustomApi => Azure Function
async function callMarvelAPI() {
  var heroName = document.getElementById("heroName").value;
  if (!heroName) {
    alert("Por favor, ingrese un nombre de héroe.");
    return;
  }

  window.parent.Xrm.Utility.showProgressIndicator("Procesando...");

  var execute_dn_LibroMarvelCustomApi_Request = {
    // Parameters
    HeroNameIn: "hulk", // Edm.String

    getMetadata: function () {
      return {
        boundParameter: null,
        parameterTypes: {
          HeroNameIn: { typeName: "Edm.String", structuralProperty: 1 },
        },
        operationType: 0,
        operationName: "dn_LibroMarvelCustomApi",
      };
    },
  };

  await window.parent.Xrm.WebApi.online
    .execute(execute_dn_LibroMarvelCustomApi_Request)
    .then(function (response) {
      if (response.ok) {
        return response.json();
      }
      throw new Error("Error en la solicitud: " + response.statusText);
    })
    .then(function (responseData) {
      displayResultsJson(responseData.HeroJsonOut);
    })
    .catch(function (error) {
      alert(error.message);
    })
    .finally(() => {
      window.parent.Xrm.Utility.closeProgressIndicator(); // ✅ Se ejecuta cuando todo termina
    });
}

async function displayResultsJson(heroJson) {
  var resultsContainer = document.getElementById("results");
  resultsContainer.innerHTML = "";

  var heroData = JSON.parse(heroJson);
  if (!heroData || !heroData.data || heroData.data.results.length === 0) {
    resultsContainer.innerHTML = "<p>No se encontraron resultados.</p>";
    return;
  }

  heroData.data.results.forEach((hero) => {
    var card = document.createElement("div");
    card.className = "card";
    card.innerHTML = `
                <img src="${hero.thumbnail.path}.${
      hero.thumbnail.extension
    }" alt="${hero.name}">
                <h3>${hero.name}</h3>
                <p>${hero.description || "Sin descripción disponible."}</p>
                <a href="${
                  hero.urls[0].url
                }" target="_blank">Más información</a>
            `;
    resultsContainer.appendChild(card);
  });
}

//Consumo con api directo
async function searchHero() {
  const name = document.getElementById("heroName").value;
  if (!name) return alert("Ingrese un nombre de héroe");

  const publicKey = await obtenerVariableEntorno("MARVEL_APIKEY_PUBLIC");
  const hash = await obtenerVariableEntorno("MARVEL_HASH_CONST");
  const timestamp = await obtenerVariableEntorno("MARVEL_TIMESTAMP");

  if (!publicKey || !hash || !timestamp) {
    alert("No se pudieron obtener las claves de la API");
    return;
  }

  const apiUrl = `https://gateway.marvel.com/v1/public/characters?nameStartsWith=${name}&ts=${timestamp}&apikey=${publicKey}&hash=${hash}`;

  try {
    const response = await fetch(apiUrl);
    const data = await validateMarvelResponse(response);

    displayResults(data.data.results);
  } catch (error) {
    console.error("Error al obtener datos:", error);
  }
}

function displayResults(heroes) {
  const resultsDiv = document.getElementById("results");
  resultsDiv.innerHTML = "";

  if (heroes.length === 0) {
    resultsDiv.innerHTML = "<p>No se encontraron héroes.</p>";
    return;
  }

  heroes.forEach((hero) => {
    const card = document.createElement("div");
    card.className = "card";
    card.innerHTML = `
                    <img src="${hero.thumbnail.path}.${
      hero.thumbnail.extension
    }" alt="${hero.name}">
                    <h3>${hero.name}</h3>
                    <p>${hero.description || "Sin descripción disponible."}</p>
                    <a href="${hero.urls[0].url}" target="_blank">Más info</a>
                `;
    resultsDiv.appendChild(card);
  });
}

function obtenerVariableEntorno(nombreVariable) {
  return new Promise((resolve, reject) => {
    window.parent.Xrm.WebApi.retrieveMultipleRecords(
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

async function validateMarvelResponse(response) {
  if (response === null) {
    console.log("Reponse Null");
    throw new Error("Reponse Null");
  }
  const responseJson = await response.json();

  if (responseJson.code !== 200) {
    OpenDialog(
      `Response Invalido: ${responseJson.code} - ${resresponseJsonponse.message}`,
      "ERROR"
    );
    console.log(responseJson);
    throw new Error("Response Marvel Invalido!");
  }

  if (responseJson.data === null) {
    OpenDialog(`Response vacio`, "ERROR");
    console.log(responseJson);
    throw new Error("Response vacio");
  }

  if (responseJson.data.results === null) {
    OpenDialog(`La consulta no obtuvo resultados`, "ERROR");
    console.log(responseJson);
    throw new Error("La consulta no obtuvo resultados");
  }

  return responseJson;
}

function OpenDialog(message, level) {
  var alertStrings = {
    confirmButtonLabel: "De Acuerdo",
    text: message,
    title: level,
  };
  var alertOptions = { height: 120, width: 260 };
  Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
    function (success) {
      console.log("Alert dialog closed");
    },
    function (error) {
      console.log(error.message);
    }
  );
}
