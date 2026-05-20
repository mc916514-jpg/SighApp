// SIGH - Módulo de Médicos (AJAX & SweetAlert2)

$(document).ready(function () {
    let debounceTimer;

    function filtrarMedicos() {
        const searchQuery = $('#medicoSearch').val();
        const especId = $('#especialidadFilter').val();

        $('#medicoTableContainer').css('opacity', '0.5');

        $.ajax({
            url: '/Medicos',
            type: 'GET',
            data: { 
                especialidadId: especId, 
                search: searchQuery, 
                isAjax: true 
            },
            success: function (data) {
                $('#medicoTableContainer').html(data);
                $('#medicoTableContainer').css('opacity', '1');

                // Actualizar badge de recuento
                const count = $('#actualMedicoCountValue').text() || '0';
                $('#medicoCount').text(count + ' médicos');
            },
            error: function () {
                $('#medicoTableContainer').css('opacity', '1');
                Swal.fire({
                    icon: 'error',
                    title: 'Error de red',
                    text: 'No se pudieron filtrar los médicos.'
                });
            }
        });
    }

    // 1. Filtrar al escribir (debounce 300ms)
    $('#medicoSearch').on('keyup input', function () {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(filtrarMedicos, 300);
    });

    // 2. Filtrar al cambiar especialidad
    $('#especialidadFilter').on('change', filtrarMedicos);

    // 3. Dar de baja / Eliminar AJAX
    $(document).on('click', '.btn-delete-medico', function () {
        const id = $(this).data('id');
        const name = $(this).data('name');

        Swal.fire({
            title: '¿Está seguro de dar de baja a ' + name + '?',
            text: "Esta acción restringirá la creación de nuevas citas para este médico.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#ef4444',
            cancelButtonColor: '#64748b',
            confirmButtonText: 'Sí, dar de baja',
            cancelButtonText: 'Cancelar',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Medicos/Delete/' + id,
                    type: 'POST',
                    success: function (response) {
                        if (response.success) {
                            // Éxito: Animación de desvanecimiento
                            $('#row-medico-' + id).fadeOut(400, function () {
                                $(this).remove();
                                
                                // Recalcular recuento
                                const currentCountText = $('#medicoCount').text();
                                let currentCount = parseInt(currentCountText) || 0;
                                if (currentCount > 0) {
                                    currentCount--;
                                    $('#medicoCount').text(currentCount + ' médicos');
                                }
                            });

                            Swal.fire({
                                icon: 'success',
                                title: 'Dar de Baja',
                                text: response.message,
                                timer: 2000,
                                showConfirmButton: false
                            });
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'Error al dar de baja',
                                text: response.message
                            });
                        }
                    },
                    error: function () {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error de red',
                            text: 'No se pudo procesar la solicitud.'
                        });
                    }
                });
            }
        });
    });
});
