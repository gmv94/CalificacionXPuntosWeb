// Helper function para SweetAlert2 que retorna un boolean
window.swalConfirm = async function(options) {
    const result = await Swal.fire(options);
    return result.isConfirmed || false;
};

