function devolver(formContext) {
  console.log("Devolver: Comando desarrollado desde ribbon");

  if (formContext.ui.getFormType() === 1) {
    OpenDialog("No se puede aplicar 'Devolver' en registros aun no guardados en dataverse!", "¡Atencion!");
    return;
  }

  var alquilerIdEntity = formContext.data.entity.getId();
  var alquilerId = alquilerIdEntity.toString().replace(/[{}]/g, "");

  if (!alquilerId || alquilerId === 0) {
    OpenDialog("No se puede aplicar 'Devolver' en registros aun no guardados en dataverse!", "¡Atencion!");
    return;
  }

  Xrm.Utility.showProgressIndicator("Procesando...");

  var execute_dn_Devolver_Request = {
    Alquiler: {
      "@odata.type": "Microsoft.Dynamics.CRM.dn_alquiler",
      dn_alquilerid: alquilerId,
    },
    getMetadata: function () {
      return {
        boundParameter: null,
        parameterTypes: {
          Alquiler: { typeName: "mscrm.dn_alquiler", structuralProperty: 5 },
        },
        operationType: 0,
        operationName: "dn_Devolver",
      };
    },
  };

  Xrm.WebApi.execute(execute_dn_Devolver_Request)
    .then(function success(response) {
      if (response.ok) return response.json();
    })
    .then(function (responseBody) {
      var result = responseBody;
      console.log(result);
      //var devolucionOK = result["DevolucionOK"];
      var devolucionOK = result.DevolucionOK;

      if (devolucionOK) {
        console.log("libro devuelto > ", devolucionOK);
        formContext.data.refresh(true);
      } else {
        console.log("el libro fue devuelto anteriormente > ", devolucionOK);
        OpenDialog("El libro fue devuelto anteriormente", "Error al devolver");
      }
    })
    .catch(function (error) {
      console.log(error.message);
    })
    .finally(() => {
      Xrm.Utility.closeProgressIndicator(); // ✅ Se ejecuta cuando todo termina
    });
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
