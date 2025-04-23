const TIMEOUT = 500;
const ERROR = "ERROR";
const ERRORID = "ERRORID";
var ErrorFields = false;
var ErrorISBN = false;

function OnLoad(context) {
  disableFields(context);
}

function OnSave(context) {
  requiredFields(context);

  if (ErrorISBN || ErrorFields) {
    context.getEventArgs().preventDefault();
  }
}

//Controls
function OnChange(context) {
  clearNotificationControls(context);
  checkISBN(context);
  requiredFields(context);
}

function disableFields(context) {
  var formContext = context.getFormContext();

  if (formContext.ui.getFormType() === 2) {
    var isbn = formContext.getControl("dn_isbn");
    if (isbn) isbn.setDisabled(true);
  }
}

function requiredFields(context) {
  var formContext = context.getFormContext();
  if (formContext.ui.getFormType() !== 1) return;

  var nombreValue = formContext.getAttribute("dn_nombre").getValue();
  var apellidoValue = formContext.getAttribute("dn_autor").getValue();
  var isbnValue = formContext.getAttribute("dn_isbn").getValue();
  var catValue = formContext.getAttribute("dn_categoria").getValue();
  var errors = [];

  if (isEmpty(nombreValue)) errors.push("Nombre");
  if (isEmpty(apellidoValue)) errors.push("Autor");
  if (isEmpty(catValue)) errors.push("Categoria");
  if (isEmpty(isbnValue)) errors.push("ISBN");

  if (errors.length !== 0) {
    formContext.ui.setFormNotification(
      "Valores ingresados en " + errors.join(", ") + " no pueden ser vacíos",
      ERROR,
      ERRORID
    );
    ErrorFields = true;
  }
}

function checkISBN(context) {
  var formContext = context.getFormContext();
  if (formContext.ui.getFormType() !== 1) return;

  var isbnAttr = formContext.getAttribute("dn_isbn");
  var isbn = isbnAttr.getValue();

  if (!isbn) return;

  formContext.ui.clearFormNotification("dn_isbn");
  ErrorISBN = false;

  Xrm.WebApi.retrieveMultipleRecords("dn_libro", "?$filter=dn_isbn eq " + isbn)
    .then(function success(results) {
      if (results && results.entities && results.entities.length > 0) {
        ErrorISBN = true;
        formContext.ui.setFormNotification(
          `ISBN ${isbn} ya se encuentra en uso!`,
          ERROR,
          "dn_isbn"
        );
      }
    })
    .catch(function (error) {
      formContext.ui.setFormNotification(error.message, ERROR, "dn_isbn");
    });
}

function clearNotificationControls(context) {
  var formContext = context.getFormContext();
  if (formContext.ui.getFormType() !== 1) return;

  var dni = formContext.getControl("dn_isbn");
  var nombre = formContext.getControl("dn_nombre");
  var autor = formContext.getControl("dn_autor");
  var cat = formContext.getControl("dn_categoria");

  if (ErrorISBN && dni.getValue()) {
    formContext.ui.clearFormNotification("dn_isbn");
    ErrorISBN = false;
  }

  if (
    ErrorFields &&
    (nombre.getValue() || autor.getValue() || cat.getValue())
  ) {
    formContext.ui.clearFormNotification(ERRORID);
    ErrorFields = false;
  }
}

function isEmpty(str) {
  return !str || str.length === 0;
}

// Ejecutar la función

// Llamar a la función
