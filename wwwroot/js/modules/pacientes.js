// SIGH - Módulo de Pacientes (AJAX & SweetAlert2)

$(document).ready(function () {
    let debounceTimer;

    // 1. Búsqueda en tiempo real con Debounce de 300ms
    $('#pacienteSearch').on('keyup input', function () {
        clearTimeout(debounceTimer);
        const searchQuery = $(this).val();

        debounceTimer = setTimeout(function () {
            // Mostrar spinner o estado de carga sutil en la tabla
            $('#pacienteTableContainer').css('opacity', '0.5');

            $.ajax({
                url: '/Pacientes',
                type: 'GET',
                data: { search: searchQuery, isAjax: true },
                success: function (data) {
                    // Cargar contenido parcial
                    $('#pacienteTableContainer').html(data);
                    $('#pacienteTableContainer').css('opacity', '1');

                    // Actualizar contador
                    const count = $('#actualCountValue').text() || '0';
                    $('#pacienteCount').text(count + ' registrados');
                },
                error: function () {
                    $('#pacienteTableContainer').css('opacity', '1');
                    Swal.fire({
                        icon: 'error',
                        title: 'Error de red',
                        text: 'No se pudo realizar la búsqueda en tiempo real.'
                    });
                }
            });
        }, 300);
    });

    // 2. Eliminación Asíncrona (AJAX) con SweetAlert2
    $(document).on('click', '.btn-delete-paciente', function () {
        const id = $(this).data('id');
        const name = $(this).data('name');
        
        Swal.fire({
            title: '¿Está seguro de eliminar a ' + name + '?',
            text: "Esta acción es irreversible y podría afectar citas médicas o historiales registrados si la base de datos lo restringe.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#ef4444', // Rojo suave danger
            cancelButtonColor: '#64748b',  // Gris neutro
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                // Realizar llamada AJAX POST
                $.ajax({
                    url: '/Pacientes/Delete/' + id,
                    type: 'POST',
                    success: function (response) {
                        if (response.success) {
                            // Éxito: Animación de desvanecimiento
                            $('#row-paciente-' + id).fadeOut(400, function () {
                                $(this).remove();
                                
                                // Recalcular recuento
                                const currentCountText = $('#pacienteCount').text();
                                let currentCount = parseInt(currentCountText) || 0;
                                if (currentCount > 0) {
                                    currentCount--;
                                    $('#pacienteCount').text(currentCount + ' registrados');
                                }
                            });

                            Swal.fire({
                                icon: 'success',
                                title: 'Eliminado',
                                text: response.message,
                                timer: 2000,
                                showConfirmButton: false
                            });
                        } else {
                            // Falló por restricción de clave foránea u otro
                            Swal.fire({
                                icon: 'error',
                                title: 'No se pudo eliminar',
                                text: response.message
                            });
                        }
                    },
                    error: function () {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error de red',
                            text: 'No se pudo procesar la solicitud de eliminación.'
                        });
                    }
                });
            }
        });
    });
});
