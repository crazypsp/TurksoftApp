// wwwroot/js/pages/Lists/TaxList.js
import { TaxApi } from '../Entites/index.js'; // dikkat: doğru dizin
$(document).ready(function () {
    const grid = $('#grid_taxlist tbody');
    const modal = $('#mdlTax');
    const form = $('#frmTax');

    // Form alanları
    const fmId = $('#taxId');
    const fmName = $('#taxName');
    const fmDesc = $('#taxDesc');
    const fmRate = $('#taxRate');
    const fmCreatedAt = $('#taxCreatedAt');
    const fmUpdatedAt = $('#taxUpdatedAt');

    async function loadTable() {
        try {
            const data = await TaxApi.list();
            grid.html((data || []).map(t => `
                <tr data-id="${t.id}">
                    <td>${t.id}</td>
                    <td>${t.name || ''}</td>
                    <td>${t.desc || ''}</td>
                    <td>${t.rate != null ? t.rate + ' %' : ''}</td>
                    <td>${t.createdAt ? new Date(t.createdAt).toLocaleDateString('tr-TR') : ''}</td>
                    <td>${t.updatedAt ? new Date(t.updatedAt).toLocaleDateString('tr-TR') : ''}</td>
                    <td class="text-center" style="width:120px;">
                        <button class="btn btn-warning btn-sm act-edit"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-danger btn-sm act-del"><i class="fa fa-trash"></i></button>
                    </td>
                </tr>
            `).join(''));
            bindActions();
        } catch (err) {
            console.error('Vergi listesi yüklenemedi:', err);
            alert(err.message || 'Liste yüklenemedi.');
        }
    }

    function bindActions() {
        $('.act-edit').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            const t = await TaxApi.get(id);
            fmId.val(t.id);
            fmName.val(t.name);
            fmDesc.val(t.desc);
            fmRate.val(t.rate);
            fmCreatedAt.val(t.createdAt ? new Date(t.createdAt).toISOString().split('T')[0] : '');
            fmUpdatedAt.val(t.updatedAt ? new Date(t.updatedAt).toISOString().split('T')[0] : '');
            modal.modal('show');
        });

        $('.act-del').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            if (!confirm('Bu vergi kaydı silinsin mi?')) return;
            await TaxApi.remove(id);
            await loadTable();
        });
    }

    $('#btnNewTax').on('click', () => {
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
            desc: fmDesc.val(),
            rate: parseFloat(fmRate.val() || 0),
            createdAt: fmCreatedAt.val() ? new Date(fmCreatedAt.val()).toISOString() : null,
            updatedAt: fmUpdatedAt.val() ? new Date(fmUpdatedAt.val()).toISOString() : null
        };

        try {
            if (dto.id) await TaxApi.update(dto.id, dto);
            else await TaxApi.create(dto);
            modal.modal('hide');
            await loadTable();
        } catch (err) {
            console.error('Kayıt hatası:', err);
            alert(err.message || 'Kayıt yapılamadı.');
        }
    });

    loadTable();
});
