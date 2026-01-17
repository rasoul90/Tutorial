(() => {
    const commandPaletteModal = document.getElementById('commandPalette');
    const commandButton = document.getElementById('command-palette-btn');

    if (commandButton && commandPaletteModal) {
        commandButton.addEventListener('click', () => {
            const modal = new bootstrap.Modal(commandPaletteModal);
            modal.show();
        });

        document.addEventListener('keydown', (event) => {
            if (event.ctrlKey && event.key.toLowerCase() === 'k') {
                event.preventDefault();
                const modal = new bootstrap.Modal(commandPaletteModal);
                modal.show();
            }
        });
    }

    document.querySelectorAll('[data-confirm]').forEach((element) => {
        element.addEventListener('click', (event) => {
            event.preventDefault();
            const message = element.getAttribute('data-confirm') || 'Are you sure?';
            Swal.fire({
                title: message,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Confirm',
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location = element.getAttribute('href') ?? '#';
                }
            });
        });
    });
})();
