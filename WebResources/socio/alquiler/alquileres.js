function OnLoad(context) {
  var formContext = context.getFormContext();
  formContext.getControl("dn_libro").setDisabled(true);

  if (formContext.ui.getFormType() === 1) {
    formContext.getAttribute("dn_libro").setValue(null);
  }
}

function OnChange(context) {
  var formContext = context.getFormContext();
  formContext.getAttribute("dn_libro").setValue(null);
  onBibliotecaChange(context);
}

function onBibliotecaChange(executionContext) {
  var formContext = executionContext.getFormContext();
  var bibliotecaCtrl = formContext.getAttribute("dn_biblioteca");
  var bibliotecaLookup = bibliotecaCtrl.getValue();
  var librosAttr = formContext.getAttribute("dn_libro");
  var librosControl = formContext.getControl("dn_libro");

  if (!bibliotecaLookup || bibliotecaLookup.length === 0) {
    librosAttr.setValue(null);
    librosControl.setDisabled(true);
    return;
  }

  var bibliotecaId = bibliotecaLookup[0].id.replace(/[{}]/g, "");

  if (bibliotecaId) {
    librosControl.setDisabled(false);

    var entityName = "dn_libro";
    var viewId = "{00000000-0000-0000-0000-000000000001}";
    var viewDisplayName = "Libros Disponibles";
    var fetchXml = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
    <entity name="dn_libro">
        <attribute name="dn_nombre" />
        <attribute name="dn_libroid" />
        <link-entity name="dn_bibliotecalibro" from="dn_libro" to="dn_libroid" alias="bl">
            <filter>
                <condition attribute="dn_biblioteca" operator="eq" value="${bibliotecaId}" />
                <condition attribute="dn_unidades" operator="gt" value="0" />
            </filter>
        </link-entity>
    </entity>
    </fetch>`;

    var layoutXml = `
    <grid name='resultset' object='1' jump='dn_libroid' select='1' icon='1' preview='1'>
      <row name='result' id='dn_libroid'>
        <cell name='dn_nombre' width='300' />
      </row>
    </grid>`;

    librosControl.addCustomView(
      viewId,
      entityName,
      viewDisplayName,
      fetchXml,
      layoutXml,
      true
    );
  } else {
    librosControl.setDisabled(true);
    librosAttr.setValue(null);
  }
}

function isEmpty(str) {
  return !str || str.length === 0;
}
