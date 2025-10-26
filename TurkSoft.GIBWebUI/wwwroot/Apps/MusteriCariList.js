// wwwroot/js/pages/Lists/MusteriCariList.js
import { CustomerApi } from '../Entites/index.js'; // dikkat: doğru dizin

$(document).ready(function () {
    const grid = $('#grid_cari tbody');
    const modal = $('#mdlCustomer');
    const form = $('#frmCustomer');

    // Form alanları
    const fmId = $('#cusId');
    const fmName = $('#cusName');
    const fmSurname = $('#cusSurname');
    const fmPhone = $('#cusPhone');
    const fmEmail = $('#cusEmail');
    const fmTaxNo = $('#cusTaxNo');
    const fmTaxOffice = $('#cusTaxOffice');
    const fmCreatedAt = $('#cusCreatedAt');
    const fmUpdatedAt = $('#cusUpdatedAt');

    async function loadTable() {
        try {
            const data = await CustomerApi.list();
            grid.html((data || []).map(c => `
                <tr data-id="${c.id}">
                    <td>${c.id}</td>
                    <td>${c.taxNo || ''}</td>
                    <td>${c.taxOffice || ''}</td>
                    <td>${c.name || ''}</td>
                    <td>${c.surname || ''}</td>
                    <td>${c.phone || ''}</td>
                    <td>${c.email || ''}</td>
                    <td>${c.createdAt ? new Date(c.createdAt).toLocaleDateString('tr-TR') : ''}</td>
                    <td class="text-center" style="width:120px;">
                        <button class="btn btn-warning btn-sm act-edit"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-danger btn-sm act-del"><i class="fa fa-remove"></i></button>
                    </td>
                </tr>
            `).join(''));
            bindActions();
        } catch (err) {
            console.error('Müşteri listesi yüklenemedi:', err);
            alert(err.message || 'Liste yüklenemedi.');
        }
    }

    function bindActions() {
        $('.act-edit').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            const c = await CustomerApi.get(id);
            fmId.val(c.id);
            fmName.val(c.name);
            fmSurname.val(c.surname);
            fmPhone.val(c.phone);
            fmEmail.val(c.email);
            fmTaxNo.val(c.taxNo);
            fmTaxOffice.val(c.taxOffice);
            fmCreatedAt.val(c.createdAt ? new Date(c.createdAt).toISOString().split('T')[0] : '');
            fmUpdatedAt.val(c.updatedAt ? new Date(c.updatedAt).toISOString().split('T')[0] : '');
            modal.modal('show');
        });

        $('.act-del').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            if (!confirm('Bu müşteri silinsin mi?')) return;
            await CustomerApi.remove(id);
            await loadTable();
        });
    }

    $('#btnNewCustomer').on('click', () => {
        form[0].reset();
        fmId.val('');
        fmCreatedAt.val(new Date().toISOString().split('T')[0]);
        fmUpdatedAt.val(new Date().toISOString().split('T')[0]);
        modal.modal('show');
    });

    form.on('submit', async e => {
        e.preventDefault();
        const dto = {
            id: fmId.val() || undefined,
            name: fmName.val(),
            surname: fmSurname.val(),
            phone: fmPhone.val(),
            email: fmEmail.val(),
            taxNo: fmTaxNo.val(),
            taxOffice: fmTaxOffice.val(),
            createdAt: fmCreatedAt.val() ? new Date(fmCreatedAt.val()).toISOString() : null,
            updatedAt: fmUpdatedAt.val() ? new Date(fmUpdatedAt.val()).toISOString() : null
        };

        try {
            if (dto.id) await CustomerApi.update(dto.id, dto);
            else await CustomerApi.create(dto);
            modal.modal('hide');
            await loadTable();
        } catch (err) {
            console.error('Kayıt hatası:', err);
            alert(err.message || 'Kayıt yapılamadı.');
        }
    });

    loadTable();
});
