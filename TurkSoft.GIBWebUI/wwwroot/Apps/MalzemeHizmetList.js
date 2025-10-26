// wwwroot/js/pages/Lists/MalzemeHizmetList.js
import { ItemApi } from '../Entites/index.js'; // dikkat: doğru dizin
// jQuery global yüklü (AdminLTE zaten getiriyor)
// document.querySelector değil, direkt jQuery kullanacağız
const $$ = sel => $(sel).toArray(); // sadece çoklu kullanım için yardımcı

$(document).ready(function () {
    const grid = $('#grid_items tbody');
    const fCode = $('#fltCode');
    const fName = $('#fltName');
    const btnFilter = $('#btnFilter');
    const btnNew = $('#btnNewItem');
    const formEl = $('#frmItem');
    const modal = $('#mdlItem');

    const fmId = $('#itmId');
    const fmCode = $('#itmCode');
    const fmName = $('#itmName');
    const fmUnit = $('#itmUnit');
    const fmPrice = $('#itmPrice');
    const fmCurrency = $('#itmCurrency');

    async function loadTable() {
        try {
            const data = await ItemApi.list();
            const codeFilter = (fCode.val() || '').toLowerCase();
            const nameFilter = (fName.val() || '').toLowerCase();
            const filtered = (data || []).filter(x =>
                (!codeFilter || (x.code || '').toLowerCase().includes(codeFilter)) &&
                (!nameFilter || (x.name || '').toLowerCase().includes(nameFilter))
            );

            const rows = (filtered || []).map(i => `
              <tr>
                <td>${i.code || ''}</td>
                <td>${i.name || ''}</td>
                <td>${i.unit?.name || ''}</td>
                <td>%18</td>
                <td>${(i.price ?? 0).toLocaleString('tr-TR', { style: 'currency', currency: i.currency || 'TRY' })}</td>
                <td class="text-end">
                  <button class="btn btn-sm btn-primary act-edit" data-id="${i.id}"><i class="fa fa-edit"></i></button>
                  <button class="btn btn-sm btn-danger act-del" data-id="${i.id}"><i class="fa fa-trash"></i></button>
                </td>
              </tr>
            `).join('');

            grid.html(rows);
            bindRowActions();
        } catch (err) {
            console.error('Liste yüklenemedi:', err);
            alert(err.message || 'Liste yüklenemedi.');
        }
    }

    function bindRowActions() {
        $('.act-edit').off('click').on('click', async function () {
            const id = $(this).data('id');
            const r = await ItemApi.get(id);
            fmId.val(r.id);
            fmCode.val(r.code || '');
            fmName.val(r.name || '');
            fmUnit.val(r.unit?.name || '');
            fmPrice.val(r.price || '');
            fmCurrency.val(r.currency || 'TRY');
            modal.modal('show');
        });

        $('.act-del').off('click').on('click', async function () {
            const id = $(this).data('id');
            if (!confirm('Bu ürün silinsin mi?')) return;
            await ItemApi.remove(id);
            await loadTable();
        });
    }

    // Yeni ürün ekle
    btnNew.on('click', function () {
        formEl[0].reset();
        fmId.val('');
        modal.modal('show');
    });

    // Kaydet (Ekle / Güncelle)
    formEl.on('submit', async function (e) {
        e.preventDefault();
        const dto = {
            id: fmId.val() || undefined,
            code: fmCode.val(),
            name: fmName.val(),
            brand: { name: $('#itmBrand').val() },
            unit: { name: fmUnit.val() },
            vatRate: parseFloat($('#itmVatRate').val() || 0),
            price: parseFloat(fmPrice.val() || 0),
            currency: fmCurrency.val()
        };
        try {
            if (dto.id) await ItemApi.update(dto.id, dto);
            else await ItemApi.create(dto);
            modal.modal('hide');
            await loadTable();
        } catch (err) {
            console.error('Kayıt hatası:', err);
            alert(err.message || 'Kayıt yapılamadı.');
        }
    });

    // Filtreleme
    fCode.on('input', loadTable);
    fName.on('input', loadTable);
    btnFilter.on('click', loadTable);

    loadTable();
});
