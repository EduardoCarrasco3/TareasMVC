function manejarClickAgregarPaso()
{
    tareaEditarViewModel.pasos.push(new pasoViewModel({ modoEdicion: false, realizado: false, descripcion: 'paso 1' }));
    $("[name='txtPasoDescripcion']:visible").focus();
}