// wwwroot/js/pages/EInvoice/Drafts.js
import { DraftInvoice } from '../Entites/DraftInvoice.js';

$(document).ready(function () {
    console.log('📄 Taslak e-Fatura ekranı yüklendi.');

    const grid = $('#draftInvoiceTable tbody');

    // === Tabloyu yükle ===
    async function loadTable() {
        try {
            const data = await DraftInvoice.list();
            grid.html((data || []).map(d => `
                <tr data-id="${d.id}">
                    <td><input type="checkbox" class="dt-checkboxes" data-id="${d.id}"/></td>
                    <td>${d.invoiceNo || ''}</td>
                    <td>${d.customer?.name || ''}</td>
                    <td>${d.customer?.taxNo || ''}</td>
                    <td>${(d.type || 'SATIŞ')} / ${(d.scenario || 'TİCARİ')}</td>
                    <td>${formatDate(d.invoiceDate)}</td>
                    <td>${formatCurrency(d.total)} ₺</td>
                    <td><span class="label label-success">Taslak</span></td>
                    <td>
                        <button class="btn btn-info btn-sm act-view"><span class="glyphicon glyphicon-zoom-in"></span></button>
                        <button class="btn btn-warning btn-sm act-edit"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-danger btn-sm act-del"><i class="fa fa-remove"></i></button>
                        <button class="btn btn-success btn-sm act-send"><i class="fa fa-send"></i></button>
                        <button class="btn btn-primary btn-sm act-copy"><i class="fa fa-pencil"></i></button>
                    </td>
                </tr>
            `).join(''));
            bindActions();
        } catch (err) {
            console.error('Taslak faturalar yüklenemedi:', err);
            alert('Fatura listesi alınamadı.');
        }
    }

    // === Buton İşlemleri ===
    function bindActions() {
        $('.act-view').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            const inv = await DraftInvoice.get(id);
            console.log('🔍 Fatura Detay:', inv);
            toastr.info(`Fatura ${inv.invoiceNo} detayları konsola yazıldı.`);
        });

        $('.act-edit').off('click').on('click', function () {
            const id = $(this).closest('tr').data('id');
            window.location.href = `/EInvoice/Edit/${id}`;
        });

        $('.act-del').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            if (!confirm('Bu taslak silinsin mi?')) return;
            await DraftInvoice.remove(id);
            await loadTable();
        });

        $('.act-send').off('click').on('click', function () {
            const id = $(this).closest('tr').data('id');
            toastr.success(`Taslak #${id} gönderim kuyruğuna alındı.`);
        });

        $('.act-copy').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            const inv = await DraftInvoice.get(id);
            inv.id = 0; // kopya olarak yeni fatura
            const newInv = await DraftInvoice.create(inv);
            toastr.success(`Fatura ${newInv.invoiceNo || ''} kopyalandı.`);
            await loadTable();
        });
    }

    // === Arama Filtreleri ===
    $('#btnAramaYapEnter, #subeler').on('change', loadTable);

    // === Yardımcı Fonksiyonlar ===
    function formatDate(dt) {
        if (!dt) return '';
        return new Date(dt).toLocaleDateString('tr-TR');
    }

    function formatCurrency(num) {
        return (num || 0).toLocaleString('tr-TR', { minimumFractionDigits: 2 });
    }

    // === Sayfa başında yükle ===
    loadTable();
});
