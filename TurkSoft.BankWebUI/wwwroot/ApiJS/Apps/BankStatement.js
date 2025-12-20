import { BankStatementsApi } from '../entities/index.js';

const $ = s => document.querySelector(s);
const val = el => (el?.value ?? '').trim();

function pick(obj, ...keys) {
    for (const k of keys) {
        if (obj && obj[k] != null) return obj[k];
    }
    return '';
}

function isoFromDateInput(v) {
    // type="date" => yyyy-MM-dd
    if (!v) return null;
    const d = new Date(v + 'T00:00:00');
    return d.toISOString();
}

document.addEventListener('DOMContentLoaded', () => {
    // DataTable (jQuery tabanlı)
    const dt = $('#tbl-ekstre').DataTable({
        responsive: true,
        pageLength: 25,
        order: [[2, 'desc']],
        columns: [
            { title: 'Banka', data: r => pick(r, 'BNKCODE', 'bnkcode') },
            { title: 'Hesap', data: r => pick(r, 'HESAPNO', 'hesapno', 'accountNumber') },
            {
                title: 'Tarih',
                data: r => pick(r, 'PROCESSTIME', 'processTime', 'PROCESSTIMESTR', 'processTimeStr'),
                render: (data, type, row) => {
                    const raw = pick(row, 'PROCESSTIME', 'processTime');
                    if (raw) {
                        const d = new Date(raw);
                        if (!isNaN(d)) return d.toLocaleString();
                    }
                    return pick(row, 'PROCESSTIMESTR', 'processTimeStr');
                }
            },
            { title: 'Açıklama', data: r => pick(r, 'PROCESSDESC', 'processDesc') },
            { title: 'Tutar', data: r => pick(r, 'PROCESSAMAOUNT', 'processAmaount') },
            { title: 'Bakiye', data: r => pick(r, 'PROCESSBALANCE', 'processBalance') },
            { title: 'B/A', data: r => pick(r, 'PROCESSDEBORCRED', 'processDebOrCred') },
            { title: 'RefNo', data: r => pick(r, 'PROCESSREFNO', 'processRefNo') }
        ]
    });

    // UI elemanları
    const bankId = $('#bankId');
    const accountNumber = $('#accountNumber');
    const musteriNo = $('#musteriNo');
    const username = $('#username');
    const password = $('#password');
    const beginDate = $('#beginDate');
    const endDate = $('#endDate');
    const link = $('#link');
    const tLink = $('#tLink');

    const btnFetch = $('#btnFetch');
    const btnClear = $('#btnClear');

    function requireMusteriNo(bid) {
        // Albaraka=5, Vakıf=8 (senin BankIds’lerine göre)
        return bid === 5 || bid === 8;
    }

    btnClear?.addEventListener('click', () => {
        dt.clear().draw();
    });

    btnFetch?.addEventListener('click', async () => {
        const bid = parseInt(val(bankId), 10);
        const acc = val(accountNumber);

        if (!bid) return alert('Banka seçiniz.');
        if (!acc) return alert('Banka hesabı / IBAN giriniz.');

        const b = val(beginDate);
        const e = val(endDate);
        if (!b || !e) return alert('Başlangıç ve bitiş tarihi zorunlu.');

        if (requireMusteriNo(bid) && !val(musteriNo))
            return alert('Bu banka için Müşteri No zorunlu.');

        const dto = {
            bankId: bid,
            username: val(username),
            password: val(password),
            accountNumber: acc,
            beginDate: isoFromDateInput(b),
            endDate: isoFromDateInput(e),
            link: val(link) || null,
            tLink: val(tLink) || null,
            extras: {}
        };

        if (val(musteriNo)) dto.extras['musteriNo'] = val(musteriNo);

        try {
            btnFetch.disabled = true;
            btnFetch.innerText = 'Sorgulanıyor...';

            const rows = await BankStatementsApi.statement(dto);
            dt.clear();
            dt.rows.add(rows || []);
            dt.draw();
        } catch (err) {
            console.error('Ekstre çekme hatası:', err, err?.payload);
            alert((err?.payload && (err.payload.message || err.payload.title || err.payload.detail)) || err?.message || 'Ekstre alınamadı.');
        } finally {
            btnFetch.disabled = false;
            btnFetch.innerText = 'Sorgula';
        }
    });
});
