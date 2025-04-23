function OpenDialog(message, level) {
  var alertStrings = {
    confirmButtonLabel: "De Acuerdo",
    text: message,
    title: level,
  };
  var alertOptions = { height: 120, width: 260 };
  window.parent.Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
    function (success) {
      console.log("Alert dialog closed");
    },
    function (error) {
      console.log(error.message);
    }
  );
}

function validateApiRestul(response) {
  if (response === null) {
    console.log("Reponse Null");
    throw new Error("Reponse Null");
  }

  const responseJson = JSON.parse(response);

  if (responseJson.hasError || responseJson.code < 200 || response.code >= 300) {
    OpenDialog(`Response Invalido: ${responseJson.code} - ${responseJson.message}`, "ERROR");
    console.log(responseJson);
    throw new Error("Response Crm Azure Function Invalido!");
    return;
  }

  if (responseJson.data === null) {
    OpenDialog(`Response vacio`, "ERROR");
    console.log(responseJson);
    throw new Error("Response vacio");
    return;
  }

  return responseJson;
}

function formatDate(dateString) {
  let date = new Date(dateString);
  return date.toLocaleDateString("es-ES");
}
