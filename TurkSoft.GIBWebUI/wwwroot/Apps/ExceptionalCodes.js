// wwwroot/js/pages/Lists/ExceptionalCodes.js
import { InfCodeApi } from '../Entites/index.js';

$(document).ready(function () {
    const grid = $('#grid_exceptional tbody');
    const modal = $('#mdlExceptional');
    const form = $('#frmExceptional');

    // Form alanları
    const fmId = $('#excId');
    const fmCode = $('#excCode');
    const fmTitle = $('#excTitle');
    const fmSolution = $('#excSolution');
    const fmSolution2 = $('#excSolution2');
    const fmType = $('#excType');

    async function loadTable() {
        try {
            const data = await InfCodeApi.list();
            grid.html((data || []).map(e => `
                <tr data-id="${e.id}">
                    <td>${e.code || ''}</td>
                    <td>${e.title || ''}</td>
                    <td>${e.type || ''}</td>
                    <td class="text-center" style="width:120px;">
                        <button class="btn btn-warning btn-sm act-edit"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-danger btn-sm act-del"><i class="fa fa-trash"></i></button>
                    </td>
                </tr>
            `).join(''));
            bindActions();
        } catch (err) {
            console.error('İstisna kod listesi yüklenemedi:', err);
            alert(err.message || 'Liste yüklenemedi.');
        }
    }

    function bindActions() {
        $('.act-edit').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            const e = await InfCodeApi.get(id);
            fmId.val(e.id);
            fmCode.val(e.code);
            fmTitle.val(e.title);
            fmSolution.val(e.solution);
            fmSolution2.val(e.solution2);
            fmType.val(e.type);
            modal.modal('show');
        });

        $('.act-del').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            if (!confirm('Bu istisna kodu silinsin mi?')) return;
            await InfCodeApi.remove(id);
            await loadTable();
        });
    }

    $('#btnNewExceptional').on('click', () => {
        form[0].reset();
        fmId.val('');
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
            type: fmType.val()
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
