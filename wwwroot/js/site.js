// SIGH - Sistema Integral de Gestión Hospitalaria (Salud Total)

$(document).ready(function () {
    // Alternar menú lateral en dispositivos móviles
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
    });

    // Pequeñas interacciones para hover y efectos de carga
    $('form').on('submit', function () {
        if ($(this).valid && $(this).valid()) {
            const submitBtn = $(this).find('button[type="submit"]');
            submitBtn.prop('disabled', true);
            submitBtn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Procesando...');
        }
    });
});
