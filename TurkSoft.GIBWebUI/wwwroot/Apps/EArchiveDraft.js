// wwwroot/js/pages/EArchive/Drafts.js
import * as EArchiveDraft from '../Entites/EArchiveDraft.js';

$(document).ready(function () {
    console.log('📋 e-Arşiv Taslak Listesi yüklendi.');

    const grid = $('#myDataTable tbody');

    // 🔎 Arama filtreleri
    const fStartDate = $('#IssueDateStart');
    const fEndDate = $('#IssueDateStartEnd');
    const fTargetTaxNo = $('#TargetIdentifier');
    const fTargetTitle = $('#TargetTitle');

    // 📦 Tablodan seçimleri oku
    function selectedIds() {
        return grid.find('input[type="checkbox"]:checked').map((i, el) => $(el).data('id')).get();
    }

    // 🔄 Listeyi yükle
    async function loadDrafts() {
        try {
            const list = await EArchiveDraft.list();
            renderTable(list);
        } catch (err) {
            console.error('Taslaklar yüklenemedi:', err);
            alert(err.message || 'Liste yüklenemedi.');
        }
    }

    // 🧱 Tabloyu çiz
    function renderTable(data = []) {
        if (!Array.isArray(data) || data.length === 0) {
            grid.html('<tr><td colspan="9" class="text-center">Kayıt bulunamadı</td></tr>');
            return;
        }
        const rows = data.map(d => `
            <tr>
                <td><input type="checkbox" class="chkRow" data-id="${d.id}" /></td>
                <td>${d.invoiceNumber || ''}</td>
                <td>${d.customerName || ''}</td>
                <td>${d.taxNumber || ''}</td>
                <td>${(d.invoiceType || '')} / ${(d.scenario || '')}</td>
                <td>${d.issueDate ? new Date(d.issueDate).toLocaleDateString('tr-TR') : ''}</td>
                <td>${(d.totalAmount ?? 0).toLocaleString('tr-TR', { style: 'currency', currency: d.currency || 'TRY' })}</td>
                <td><span class="label label-success">${d.status || 'Taslak'}</span></td>
                <td class="text-center">
                    <button class="btn btn-info btn-xs act-view" data-id="${d.id}"><i class="glyphicon glyphicon-zoom-in"></i></button>
                    <button class="btn btn-warning btn-xs act-edit" data-id="${d.id}"><i class="fa fa-edit"></i></button>
                    <button class="btn btn-danger btn-xs act-del" data-id="${d.id}"><i class="fa fa-remove"></i></button>
                    <button class="btn btn-success btn-xs act-send" data-id="${d.id}"><i class="fa fa-send"></i></button>
                </td>
            </tr>
        `).join('');
        grid.html(rows);
        bindRowActions();
    }

    // 🎯 Satır işlemleri
    function bindRowActions() {
        $('.act-view').off('click').on('click', async function () {
            const id = $(this).data('id');
            const draft = await EArchiveDraft.get(id);
            console.table(draft);
            toastr.info('Fatura detayları konsolda görüntülendi.');
        });

        $('.act-edit').off('click').on('click', function () {
            const id = $(this).data('id');
            window.location.href = `/EArchive/Edit/${id}`;
        });

        $('.act-del').off('click').on('click', async function () {
            const id = $(this).data('id');
            if (!confirm('Bu taslak silinsin mi?')) return;
            await EArchiveDraft.remove(id);
            await loadDrafts();
        });

        $('.act-send').off('click').on('click', async function () {
            const id = $(this).data('id');
            await EArchiveDraft.update(id, { status: 'ReadyToSend' });
            toastr.success('Fatura GİB\'e gönderilmek üzere işaretlendi.');
        });
    }

    // 🔍 Arama yap
    window.gridSearch = async function () {
        const filters = {
            issueDateStart: fStartDate.val(),
            issueDateEnd: fEndDate.val(),
            targetIdentifier: fTargetTaxNo.val(),
            targetTitle: fTargetTitle.val()
        };
        const data = await EArchiveDraft.list(filters);
        renderTable(data);
    };

    // 🧹 Temizle
    window.clearSearchInputs = function () {
        fStartDate.val(''); fEndDate.val('');
        fTargetTaxNo.val(''); fTargetTitle.val('');
        loadDrafts();
    };

    // 🆕 Yeni oluştur
    window.createNewDraftArsiv = function () {
        window.location.href = '/EArchive/CreateNewEarchiveInvoice';
    };

    // 🚀 Seçilileri gönder
    window.sendAllDraft = async function () {
        const ids = selectedIds();
        if (ids.length === 0) return toastr.warning('Hiçbir taslak seçilmedi.');
        await EArchiveDraft.sendAll(ids);
        toastr.success('Seçili taslaklar gönderildi.');
        await loadDrafts();
    };

    // ❌ Seçilileri sil
    window.deleteAllDraft = async function () {
        const ids = selectedIds();
        if (ids.length === 0) return toastr.warning('Hiçbir taslak seçilmedi.');
        if (!confirm('Seçili taslaklar silinsin mi?')) return;
        await EArchiveDraft.deleteAll(ids);
        toastr.success('Seçili taslaklar silindi.');
        await loadDrafts();
    };

    // 📤 Excel’den taslak oluşturma
    window.excelDraftModalOpen = () => $('#excelDraftModal').modal('show');

    window.excelFileUpload = async function () {
        const file = $('#FileExcel')[0].files[0];
        if (!file) return toastr.warning('Excel dosyası seçiniz.');
        const formData = new FormData();
        formData.append('file', file);
        await EArchiveDraft.uploadExcel(formData);
        toastr.success('Excel dosyasından taslaklar eklendi.');
        $('#excelDraftModal').modal('hide');
        await loadDrafts();
    };

    // 🔐 İmzalama modal (şimdilik bilgi mesajı)
    $('#signDraftModal .btn-success').on('click', () => toastr.info('İmzalama işlemi backend tarafında yürütülür.'));

    // 📅 Tarih seçici
    if ($.fn.datepicker) $('.datepicker').datepicker({ language: 'tr', autoclose: true, todayHighlight: true });

    // 🔄 Başlangıçta listeyi yükle
    loadDrafts();
});
