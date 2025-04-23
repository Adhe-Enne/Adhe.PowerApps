const TIMEOUT = 500;
const ERROR = "ERROR";
const ERRORID = "ERRORID";
var ErrorFields = false;
var ErrorDNI = false;
let recordId = null;
/*Mains*/
function OnLoad(context) {
  disableFields(context);
}

function OnSave(context) {
  requiredFields(context);
  if (ErrorDNI || ErrorFields) {
    context.getEventArgs().preventDefault();
  }
}

function OnChange(context) {
  clearNotificationControls(context);
  checkDni(context);
  requiredFields(context);
}

/*Common*/
function disableFields(context) {
  var formContext = context.getFormContext();
  if (formContext.ui.getFormType() === 2) {
    SetCurrentRecordId(context);
    var dni = formContext.getControl("dn_dni");
    var nro = formContext.getControl("dn_nrodesocio");

    if (dni) dni.setDisabled(true);
    if (nro) nro.setDisabled(true);
  }

  if (formContext.ui.getFormType() !== 1) {
    formContext.getControl("WebResource_SocioAlquileres").setVisible(true);
    //formContext.getControl("WebResource_AlquilerCreate").setVisible(true);
  } else {
    formContext.getControl("WebResource_SocioAlquileres").setVisible(false);
    //formContext.getControl("WebResource_AlquilerCreate").setVisible(false);
  }
}

function requiredFields(context) {
  var formContext = context.getFormContext();
  if (formContext.ui.getFormType() !== 1) return;

  var nombreValue = formContext.getAttribute("dn_nombre").getValue();
  var apellidoValue = formContext.getAttribute("dn_apellido").getValue();
  var dniValue = formContext.getAttribute("dn_dni").getValue();
  var errors = [];

  if (isEmpty(nombreValue)) errors.push("Nombre");
  if (isEmpty(apellidoValue)) errors.push("Apellido");
  if (isEmpty(dniValue)) errors.push("DNI");

  if (errors.length !== 0) {
    formContext.ui.setFormNotification("Valores ingresados en " + errors.join(", ") + " no pueden ser vacíos", ERROR, ERRORID);
    ErrorFields = true;
  }
}

function checkDni(context) {
  var formContext = context.getFormContext();
  if (formContext.ui.getFormType() !== 1) return;

  var dniAttr = formContext.getAttribute("dn_dni");
  var dni = dniAttr.getValue();

  if (!dni) return;

  formContext.ui.clearFormNotification("dn_dni");
  ErrorDNI = false;

  // Consultamos en Dataverse si el DNI ya existe
  Xrm.WebApi.retrieveMultipleRecords("dn_socio", "?$filter=dn_dni eq " + dni)
    .then(function success(results) {
      if (results && results.entities && results.entities.length > 0) {
        ErrorDNI = true;
        formContext.ui.setFormNotification(`DNI ${dni} ya se encuentra en uso!`, ERROR, "dn_dni");
      }
    })
    .catch(function (error) {
      formContext.ui.setFormNotification(error.message, ERROR, "dn_dni");
    });
}

function clearNotificationControls(context) {
  var formContext = context.getFormContext();

  if (formContext.ui.getFormType() !== 1) return;

  var dni = formContext.getControl("dn_dni");
  var nombre = formContext.getControl("dn_nombre");
  var autor = formContext.getControl("dn_apellido");

  if (ErrorDNI && dni.getValue()) {
    formContext.ui.clearFormNotification("dn_dni"); // Eliminar notificación si ya no hay error
    ErrorDNI = false;
  }

  if (ErrorFields && (nombre.getValue() || autor.getValue())) {
    ErrorFields = false;
    formContext.ui.clearFormNotification(ERRORID); // Eliminar notificación si ya no hay error
  }
}

function isEmpty(str) {
  return !str || str.length === 0;
}

function SetCurrentRecordId(executionContext) {
  var formContext = executionContext.getFormContext(); // Obtiene el contexto del formulario
  recordId = formContext.data.entity.getId(); // Captura el ID del registro
  console.log("El ID del registro es: " + recordId);
}
