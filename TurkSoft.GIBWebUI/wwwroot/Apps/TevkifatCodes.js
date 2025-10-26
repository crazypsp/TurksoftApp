// wwwroot/js/pages/Lists/TevkifatCodes.js
import { InfCodeApi } from '../Entites/index.js';

$(document).ready(function () {
    const grid = $('#grid_Tevkifat tbody');
    const modal = $('#mdlTevkifat');
    const form = $('#frmTevkifat');

    // Form alanları
    const fmId = $('#tevId');
    const fmCode = $('#tevCode');
    const fmTitle = $('#tevTitle');
    const fmSolution = $('#tevSolution');
    const fmSolution2 = $('#tevSolution2');
    const fmType = $('#tevType');

    async function loadTable() {
        try {
            const data = await InfCodeApi.list();
            // Sadece Type="Tevkifat" olanları filtreleyelim
            const filtered = (data || []).filter(x => (x.type || '').toLowerCase() === 'tevkifat');
            grid.html((filtered || []).map(t => `
                <tr data-id="${t.id}">
                    <td>${t.code || ''}</td>
                    <td>${t.title || ''}</td>
                    <td class="text-center" style="width:120px;">
                        <button class="btn btn-warning btn-sm act-edit"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-danger btn-sm act-del"><i class="fa fa-trash"></i></button>
                    </td>
                </tr>
            `).join(''));
            bindActions();
        } catch (err) {
            console.error('Tevkifat kod listesi yüklenemedi:', err);
            alert(err.message || 'Liste yüklenemedi.');
        }
    }

    function bindActions() {
        $('.act-edit').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            const t = await InfCodeApi.get(id);
            fmId.val(t.id);
            fmCode.val(t.code);
            fmTitle.val(t.title);
            fmSolution.val(t.solution);
            fmSolution2.val(t.solution2);
            fmType.val(t.type || 'Tevkifat');
            modal.modal('show');
        });

        $('.act-del').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            if (!confirm('Bu tevkifat kodu silinsin mi?')) return;
            await InfCodeApi.remove(id);
            await loadTable();
        });
    }

    $('#btnNewTevkifat').on('click', () => {
        form[0].reset();
        fmId.val('');
        fmType.val('Tevkifat');
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
            type: fmType.val() || 'Tevkifat'
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
