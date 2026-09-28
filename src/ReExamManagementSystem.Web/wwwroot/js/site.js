// Generic wiring shared by every "Add / Edit" (modal form) and "Delete"
// (confirmation modal) screen in the Admin/Officer/Student areas, so each
// CRUD page only needs data-* attributes on its buttons, not repeated JS.
(function () {
    'use strict';

    var crudModalEl = document.getElementById('crudModal');
    var crudModal = crudModalEl ? new bootstrap.Modal(crudModalEl) : null;
    var crudModalContent = document.getElementById('crudModalContent');

    var deleteModalEl = document.getElementById('deleteModal');
    var deleteModal = deleteModalEl ? new bootstrap.Modal(deleteModalEl) : null;
    var deleteModalItemName = document.getElementById('deleteModalItemName');
    var deleteModalForm = document.getElementById('deleteModalForm');

    function loadCrudForm(url) {
        if (!crudModal || !crudModalContent) return;
        crudModalContent.innerHTML =
            '<div class="modal-body text-center py-5"><div class="spinner-border text-primary"></div></div>';
        crudModal.show();

        fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (response) { return response.text(); })
            .then(function (html) { crudModalContent.innerHTML = html; })
            .catch(function () {
                crudModalContent.innerHTML = '<div class="modal-body text-danger">Failed to load the form.</div>';
            });
    }

    document.addEventListener('click', function (e) {
        var openBtn = e.target.closest('[data-crud-url]');
        if (openBtn) {
            e.preventDefault();
            loadCrudForm(openBtn.getAttribute('data-crud-url'));
            return;
        }

        var delBtn = e.target.closest('[data-delete-url]');
        if (delBtn && deleteModal && deleteModalForm) {
            e.preventDefault();
            deleteModalItemName.textContent = delBtn.getAttribute('data-delete-name') || 'this item';
            deleteModalForm.action = delBtn.getAttribute('data-delete-url');
            deleteModal.show();
        }
    });

    document.addEventListener('submit', function (e) {
        var form = e.target.closest('#crudModalContent form');
        if (!form) return;
        e.preventDefault();

        var submitBtn = form.querySelector('button[type="submit"]');
        if (submitBtn) submitBtn.disabled = true;

        fetch(form.action, {
            method: 'POST',
            body: new FormData(form),
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(function (response) { return response.text().then(function (text) { return { response: response, text: text }; }); })
            .then(function (result) {
                var isJson = result.response.headers.get('content-type')?.indexOf('application/json') === 0;
                if (result.response.ok && isJson) {
                    window.location.reload();
                } else {
                    // Validation failed: the server re-rendered the form partial with errors.
                    crudModalContent.innerHTML = result.text;
                    if (submitBtn) submitBtn.disabled = false;
                }
            });
    });
})();
