// wwwroot/js/pages/Lists/DurumKod.js
import { InfCodeApi } from '../Entites/index.js';

$(document).ready(function () {
    const grid = $('#grid_DurumKod tbody');
    const modal = $('#mdlDurumKod');
    const form = $('#frmDurumKod');

    // Form alanları
    const fmId = $('#drmId');
    const fmCode = $('#drmCode');
    const fmTitle = $('#drmTitle');
    const fmSolution = $('#drmSolution');
    const fmSolution2 = $('#drmSolution2');
    const fmType = $('#drmType');

    async function loadTable() {
        try {
            const data = await InfCodeApi.list();
            const filtered = (data || []).filter(x => (x.type || '').toLowerCase() === 'durumkod');
            grid.html((filtered || []).map(d => `
                <tr data-id="${d.id}">
                    <td>${d.code || ''}</td>
                    <td>${d.title || ''}</td>
                    <td>${d.solution || ''}</td>
                    <td>${d.solution2 || ''}</td>
                    <td class="text-center" style="width:120px;">
                        <button class="btn btn-warning btn-sm act-edit"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-danger btn-sm act-del"><i class="fa fa-trash"></i></button>
                    </td>
                </tr>
            `).join(''));
            bindActions();
        } catch (err) {
            console.error('Durum kod listesi yüklenemedi:', err);
            alert(err.message || 'Liste yüklenemedi.');
        }
    }

    function bindActions() {
        $('.act-edit').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            const d = await InfCodeApi.get(id);
            fmId.val(d.id);
            fmCode.val(d.code);
            fmTitle.val(d.title);
            fmSolution.val(d.solution);
            fmSolution2.val(d.solution2);
            fmType.val(d.type || 'DurumKod');
            modal.modal('show');
        });

        $('.act-del').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            if (!confirm('Bu durum kodu silinsin mi?')) return;
            await InfCodeApi.remove(id);
            await loadTable();
        });
    }

    $('#btnNewDurumKod').on('click', () => {
        form[0].reset();
        fmId.val('');
        fmType.val('DurumKod');
        modal.modal('show');
    });

    form.on('submit', async e => {
        e.preventDefault();
        const dto = {
            id: fmId.val() || undefined,
            code: fmCode.val(),
            title: fmTitle.val(),
            solution: fmSolution.val(),
            solution2: fmSolution2.val(),
            type: fmType.val() || 'DurumKod'
        };

        try {
            if (dto.id) await InfCodeApi.update(dto.id, dto);
            else await InfCodeApi.create(dto);
            modal.modal('hide');
            await loadTable();
        } catch (err) {
            console.error('Kayıt hatası:', err);
            alert(err.message || 'Kayıt yapılamadı.');
        }
    });

    loadTable();
});
