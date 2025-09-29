import { UrunFiyatApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-urunfiyat-body');
  const btnNew = $('#btnNewUrunFiyat');
  const modalEl = $('#mdlUrunFiyat');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmUrunFiyat');

  // Form alanları
  const fmId = $('#frmId');
  const fmPaketId = $('#frmPaketId');
  const fmFiyat = $('#frmFiyat');
  const fmParaBirimi = $('#frmParaBirimi');
  const fmGecerlilikBaslangic = $('#frmGecerlilikBaslangic');
  const fmGecerlilikBitis = $('#frmGecerlilikBitis');

  // ---- Log helper ----
  async function logIslem(tip) {
    try {
      const session = await getSession();
      const userId = session?.Id || "0";
      const ipAdres = await fetch("https://api.ipify.org?format=json")
        .then(r => r.json())
        .then(d => d.ip)
        .catch(() => "unknown");

      const dto = {
        Islem: `${userId}-UrunFiyat-${tip}`,
        IpAdres: ipAdres,
        Tarayici: navigator.userAgent
      };
      await LogApi.create(dto);
    } catch (err) {
      console.error("Log kaydı başarısız:", err);
    }
  }

  // ---- Listeleme ----
  async function loadTable() {
    const list = await UrunFiyatApi.list();
    await logIslem("List");

    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.Fiyat ?? 0} ${r.ParaBirimi ?? ''}</td>
        <td>${r.GecerlilikBaslangic ? new Date(r.GecerlilikBaslangic).toLocaleDateString() : ''}</td>
        <td>${r.GecerlilikBitis ? new Date(r.GecerlilikBitis).toLocaleDateString() : ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del" data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');

    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await UrunFiyatApi.get(id);
    await logIslem("GetById");
    return r;
  }

  // ---- Satır aksiyonları ----
  function bindRowActions() {
    $$('.act-edit').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        const r = await getById(id);

        fmId.value = r.Id;
        fmPaketId.value = r.PaketId ?? '';
        fmFiyat.value = r.Fiyat ?? 0;
        fmParaBirimi.value = r.ParaBirimi ?? '';
        fmGecerlilikBaslangic.value = r.GecerlilikBaslangic ? r.GecerlilikBaslangic.substring(0, 10) : '';
        fmGecerlilikBitis.value = r.GecerlilikBitis ? r.GecerlilikBitis.substring(0, 10) : '';

        modal?.show();
      })
    );

    $$('.act-del').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        if (!confirm('Silinsin mi?')) return;
        await UrunFiyatApi.remove(id);
        await logIslem("Delete");
        await loadTable();
      })
    );
  }

  // ---- Yeni kayıt ----
  btnNew?.addEventListener('click', () => {
    formEl.reset();
    fmId.value = '';
    modal?.show();
  });

  // ---- Insert / Update ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();

    const dto = {
      Id: fmId.value || undefined,
      PaketId: val(fmPaketId),
      Fiyat: parseFloat(val(fmFiyat)) || 0,
      ParaBirimi: val(fmParaBirimi),
      GecerlilikBaslangic: val(fmGecerlilikBaslangic) || null,
      GecerlilikBitis: val(fmGecerlilikBitis) || null
    };

    if (dto.Id) {
      await UrunFiyatApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await UrunFiyatApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
