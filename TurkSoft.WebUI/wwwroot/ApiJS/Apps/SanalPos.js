import { SanalPosApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-sanalpos-body');
  const btnNew = $('#btnNewSanalPos');
  const modalEl = $('#mdlSanalPos');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmSanalPos');

  // Form alanları
  const fmId = $('#frmId');
  const fmBayiId = $('#frmBayiId');
  const fmSaglayici = $('#frmSaglayici');
  const fmBaseApiUrl = $('#frmBaseApiUrl');
  const fmMerchantId = $('#frmMerchantId');
  const fmApiKey = $('#frmApiKey');
  const fmApiSecret = $('#frmApiSecret');
  const fmPosAnahtar = $('#frmPosAnahtar');
  const fmStandartKomisyonYuzde = $('#frmStandartKomisyonYuzde');

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
        Islem: `${userId}-${tip}`,
        IpAdres: ipAdres,
        Tarayici: navigator.userAgent
      };
      await LogApi.create(dto);
    } catch (err) {
      console.error("Log yazılamadı:", err);
    }
  }

  // ---- Listeleme ----
  async function loadTable() {
    const list = await SanalPosApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.BayiId ?? ''}</td>
        <td>${r.Saglayici ?? ''}</td>
        <td>${r.MerchantId ?? ''}</td>
        <td>${r.StandartKomisyonYuzde ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del" data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await SanalPosApi.get(id);
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
        fmBayiId.value = r.BayiId ?? '';
        fmSaglayici.value = r.Saglayici ?? '';
        fmBaseApiUrl.value = r.BaseApiUrl ?? '';
        fmMerchantId.value = r.MerchantId ?? '';
        fmApiKey.value = r.ApiKey ?? '';
        fmApiSecret.value = r.ApiSecret ?? '';
        fmPosAnahtar.value = r.PosAnahtar ?? '';
        fmStandartKomisyonYuzde.value = r.StandartKomisyonYuzde ?? '';

        modal?.show();
      })
    );
    $$('.act-del').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        if (!confirm('Silinsin mi?')) return;
        await SanalPosApi.remove(id);
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

  // ---- Insert/Update ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = {
      Id: fmId.value || undefined,
      BayiId: val(fmBayiId) || null,
      Saglayici: val(fmSaglayici),
      BaseApiUrl: val(fmBaseApiUrl),
      MerchantId: val(fmMerchantId),
      ApiKey: val(fmApiKey),
      ApiSecret: val(fmApiSecret),
      PosAnahtar: val(fmPosAnahtar),
      StandartKomisyonYuzde: parseFloat(val(fmStandartKomisyonYuzde)) || null
    };

    if (dto.Id) {
      await SanalPosApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await SanalPosApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
