// wwwroot/js/pages/EInvoice/CreateExportInvoice.js
import { ExportInvoice } from '../Entites/ExportInvoice.js';

$(document).ready(function () {
    console.log('🌍 e-İhracat Fatura ekranı yüklendi.');

    const btnSave = $('.btn-success.pull-right');

    // Kaydet butonuna tıklama
    btnSave.off('click').on('click', async function (e) {
        e.preventDefault();
        const dto = collectInvoice();

        try {
            const res = await ExportInvoice.create(dto);
            toastr.success(`e-İhracat faturası kaydedildi (No: ${res.invoiceNumber || ''})`);
            console.log('Kaydedilen ihracat faturası:', res);
        } catch (err) {
            console.error('e-İhracat faturası kaydedilemedi:', err);
            toastr.error(err.message || 'Kayıt sırasında hata oluştu.');
        }
    });

    // === Form verilerini topla ===
    function collectInvoice() {
        const dto = {};

        dto.invoiceScenario = $('#InvoiceScenario').val() || 'IHRACAT';
        dto.invoiceDate = parseDate($('#InvoiceDate').val());
        dto.invoiceNumber = $('#InvoiceNumber').val();
        dto.buyer = $('#Buyer').val();
        dto.country = $('#Country').val();
        dto.currency = $('#Currency').val();
        dto.createdAt = new Date().toISOString();

        // Kalemler
        dto.items = [];
        $('#itemsTable tbody tr').each(function () {
            const $r = $(this);
            const ad = $r.find('td:eq(0) input').val();
            const miktar = parseFloat($r.find('td:eq(1) input').val() || 0);
            const birim = $r.find('td:eq(2) input').val();
            const fiyat = parseFloat($r.find('td:eq(3) input').val() || 0);
            const kdv = parseFloat($r.find('td:eq(4) input').val() || 0);
            const toplam = miktar * fiyat * (1 + kdv / 100);
            $r.find('td:eq(5) input').val(toplam.toFixed(2));
            dto.items.push({
                name: ad,
                quantity: miktar,
                unit: birim,
                price: fiyat,
                vatRate: kdv,
                total: toplam
            });
        });

        // Sevk/Gümrük
        dto.shipping = {
            customsOffice: $('#CustomsOffice').val(),
            transportType: $('#TransportType').val()
        };

        // Ödeme
        dto.payment = {
            method: $('#PaymentMethod').val(),
            dueDate: parseDate($('#DueDate').val())
        };

        return dto;
    }

    // === Yardımcı fonksiyonlar ===
    function parseDate(v) {
        if (!v) return new Date().toISOString();
        const parts = v.split('.');
        if (parts.length === 3)
            return new Date(parts[2], parts[1] - 1, parts[0]).toISOString();
        return new Date().toISOString();
    }

    // === Test için: kayıtlı ihracat faturalarını getir ===
    (async () => {
        try {
            const list = await ExportInvoice.list();
            console.log('📋 Kayıtlı ihracat faturaları:', list);
        } catch (err) {
            console.warn('İhracat faturaları listesi alınamadı.');
        }
    })();
});
