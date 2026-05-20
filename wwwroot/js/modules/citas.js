// SIGH - Módulo de Citas Médicas (Filtros AJAX, Cancelación y Validación de Horarios)

$(document).ready(function () {
    let debounceTimer;

    // --- FILTRADO DE CITAS DENTRO DE INDEX ---
    function filtrarCitas() {
        const fechaInicio = $('#fechaInicioFilter').val();
        const fechaFin = $('#fechaFinFilter').val();
        const medicoId = $('#medicoFilter').val();
        const especialidadId = $('#especialidadFilter').val();
        const search = $('#citaSearch').val();

        $('#citaTableContainer').css('opacity', '0.5');

        $.ajax({
            url: '/Citas',
            type: 'GET',
            data: {
                fechaInicio: fechaInicio,
                fechaFin: fechaFin,
                medicoId: medicoId,
                especialidadId: especialidadId,
                search: search,
                isAjax: true
            },
            success: function (data) {
                $('#citaTableContainer').html(data);
                $('#citaTableContainer').css('opacity', '1');

                // Actualizar badge del contador de citas
                const count = $('#actualCitaCountValue').text() || '0';
                $('#citaCount').text(count + ' citas encontradas');
            },
            error: function () {
                $('#citaTableContainer').css('opacity', '1');
                Swal.fire({
                    icon: 'error',
                    title: 'Error de red',
                    text: 'No se pudieron filtrar las citas médicas.'
                });
            }
        });
    }

    // Escuchar eventos en filtros de Index
    $('#fechaInicioFilter, #fechaFinFilter, #medicoFilter, #especialidadFilter').on('change', filtrarCitas);
    $('#citaSearch').on('keyup input', function () {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(filtrarCitas, 350);
    });


    // --- CANCELACIÓN DE CITAS MÉDICAS ---
    $(document).on('click', '.btn-cancel-cita', function () {
        const id = $(this).data('id');
        const patientName = $(this).data('patient');

        Swal.fire({
            title: '¿Confirmar cancelación?',
            text: `¿Está seguro de cancelar la cita de ${patientName}? Esta acción liberará el espacio de atención y no se puede deshacer.`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#ef4444',
            cancelButtonColor: '#64748b',
            confirmButtonText: 'Sí, cancelar cita',
            cancelButtonText: 'Mantener agendada',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Citas/Cancel/' + id,
                    type: 'POST',
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Cita Cancelada',
                                text: response.message,
                                timer: 2000,
                                showConfirmButton: false
                            });
                            // Si estamos en la vista de listado Index, recargamos filtros. Si estamos en detalles, recargamos la página.
                            if ($('#citaTableContainer').length > 0) {
                                filtrarCitas();
                            } else {
                                setTimeout(function () {
                                    location.reload();
                                }, 1500);
                            }
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'Error al Cancelar',
                                text: response.message
                            });
                        }
                    },
                    error: function () {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error de Red',
                            text: 'No se pudo procesar la solicitud de cancelación.'
                        });
                    }
                });
            }
        });
    });


    // --- VALIDACIÓN DE DISPONIBILIDAD AJAX (CREATE & EDIT) ---
    const medicoSelect = $('#medicoSelect');
    const fechaHoraInput = $('#fechaHoraInput');
    const feedbackContainer = $('#availabilityFeedback');
    const feedbackAlert = $('#feedbackAlert');
    const feedbackIcon = $('#feedbackIcon');
    const feedbackMessage = $('#feedbackMessage');
    const btnSubmit = $('#btnSubmitCita');

    function validarDisponibilidad() {
        const medicoId = medicoSelect.val();
        const fechaHora = fechaHoraInput.val();

        // Ocultar si no hay valores completos
        if (!medicoId || !fechaHora) {
            feedbackContainer.hide();
            fechaHoraInput.removeClass('is-valid is-invalid');
            btnSubmit.prop('disabled', false); // No bloquear si está incompleto (la validación del form se encargará de requerirlo)
            return;
        }

        // Mostrar caja en estado "verificando..."
        feedbackContainer.show();
        feedbackAlert.removeClass('bg-success-subtle border-success-subtle text-success bg-danger-subtle border-danger-subtle text-danger').addClass('bg-light border-slate text-muted');
        feedbackIcon.removeClass().addClass('bi bi-arrow-clockwise animate-spin me-2');
        feedbackMessage.text('Comprobando agenda del médico en tiempo real...');
        btnSubmit.prop('disabled', true);

        $.ajax({
            url: '/Citas/ValidarDisponibilidad',
            type: 'GET',
            data: {
                medicoId: medicoId,
                fechaHora: fechaHora
            },
            success: function (response) {
                feedbackIcon.removeClass('animate-spin me-2');
                if (response.isAvailable) {
                    // Disponible
                    feedbackAlert.removeClass('bg-light border-slate text-muted').addClass('bg-success-subtle border-success-subtle text-success');
                    feedbackIcon.addClass('bi bi-check-circle-fill me-2');
                    feedbackMessage.text(response.message);
                    
                    fechaHoraInput.removeClass('is-invalid').addClass('is-valid');
                    btnSubmit.prop('disabled', false);
                } else {
                    // Conflicto de horario / doble booking / inactivo / fuera de horario
                    feedbackAlert.removeClass('bg-light border-slate text-muted').addClass('bg-danger-subtle border-danger-subtle text-danger');
                    feedbackIcon.addClass('bi bi-exclamation-triangle-fill me-2');
                    feedbackMessage.text(response.message);
                    
                    fechaHoraInput.removeClass('is-valid').addClass('is-invalid');
                    btnSubmit.prop('disabled', true); // BLOQUEAR AGENDAMIENTO
                }
            },
            error: function () {
                feedbackIcon.removeClass('animate-spin me-2');
                feedbackAlert.removeClass('bg-light border-slate text-muted').addClass('bg-danger-subtle border-danger-subtle text-danger');
                feedbackIcon.addClass('bi bi-exclamation-circle-fill me-2');
                feedbackMessage.text('Error al verificar la disponibilidad de horario.');
                
                fechaHoraInput.removeClass('is-valid').addClass('is-invalid');
                btnSubmit.prop('disabled', false); // Permitir en caso de falla de red (degradación graciosa)
            }
        });
    }

    // Escuchar cambios de médico y fecha/hora
    medicoSelect.on('change', validarDisponibilidad);
    fechaHoraInput.on('change keyup', function () {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(validarDisponibilidad, 500);
    });
});
