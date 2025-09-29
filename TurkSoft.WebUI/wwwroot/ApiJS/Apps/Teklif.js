import { TeklifApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-teklif-body');
  const btnNew = $('#btnNewTeklif');
  const modalEl = $('#mdlTeklif');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmTeklif');

  // Form alanları
  const fmId = $('#frmId');
  const fmTeklifNo = $('#frmTeklifNo');
  const fmKdvoran = $('#frmKdvoran');
  const fmKdvtutar = $('#frmKdvtutar');
  const fmToplam = $('#frmToplam');
  const fmNet = $('#frmNet');
  const fmDurum = $('#frmDurum');
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
        Islem: `${userId}-Teklif-${tip}`,
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
    const list = await TeklifApi.list();
    await logIslem("List");

    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.TeklifNo ?? ''}</td>
        <td>${r.Toplam ?? 0}</td>
        <td>${r.Durum ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del" data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');

    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await TeklifApi.get(id);
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
        fmTeklifNo.value = r.TeklifNo ?? '';
        fmKdvoran.value = r.Kdvoran ?? '';
        fmKdvtutar.value = r.Kdvtutar ?? '';
        fmToplam.value = r.Toplam ?? '';
        fmNet.value = r.Net ?? '';
        fmDurum.value = r.Durum ?? 0;
        fmGecerlilikBitis.value = r.GecerlilikBitis ? r.GecerlilikBitis.split('T')[0] : '';

        modal?.show();
      })
    );

    $$('.act-del').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        if (!confirm('Silinsin mi?')) return;
        await TeklifApi.remove(id);
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
      TeklifNo: val(fmTeklifNo),
      Kdvoran: parseFloat(val(fmKdvoran)) || 0,
      Kdvtutar: parseFloat(val(fmKdvtutar)) || 0,
      Toplam: parseFloat(val(fmToplam)) || 0,
      Net: parseFloat(val(fmNet)) || 0,
      Durum: parseInt(val(fmDurum)) || 0,
      GecerlilikBitis: val(fmGecerlilikBitis) || null
    };

    if (dto.Id) {
      await TeklifApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await TeklifApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
