// wwwroot/apps/einvoice/UploadTransferInvoice.js
import { EinvoiceTransferApi } from '../Base/turkcellEfaturaApi.js';

(function () {
    'use strict';

    const MAX_MB = 200;

    const elForm = document.getElementById('uploadForm');
    const elInfo0 = document.getElementById('info_files_0');
    const elInfo1 = document.getElementById('info_files_1');
    const elLbl = document.getElementById('lbl_bilgi');

    const elInp0 = document.getElementById('files_0');
    const elInp1 = document.getElementById('files_1');

    const elDz0 = document.getElementById('dz_invoice');
    const elDz1 = document.getElementById('dz_envelope');

    const elIntegrator = document.getElementById('entegrator');
    const elTip = document.getElementById('tip');

    const elBtnClear = document.getElementById('btnClear');
    const elBtnUpload = document.getElementById('Upload');

    const state = {
        invoiceZip: null,
        envelopeZip: null
    };

    function ok(m) {
        if (window.toastr?.success) toastr.success(m);
        if (elLbl) elLbl.textContent = m || '';
    }
    function err(m) {
        if (window.toastr?.error) toastr.error(m);
        if (elLbl) elLbl.textContent = m || '';
    }

    function bytesToSize(bytes) {
        if (!bytes) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return (bytes / Math.pow(k, i)).toFixed(i ? 1 : 0) + ' ' + sizes[i];
    }

    function isZip(file) {
        return !!file && /\.zip$/i.test(file.name || '');
    }
    function isSizeOk(file) {
        return !!file && file.size <= MAX_MB * 1024 * 1024;
    }

    function setInvalid(el, v) {
        if (!el) return;
        if (v) el.classList.add('is-invalid');
        else el.classList.remove('is-invalid');
    }

    function renderInfo(which) {
        const file = which === 0 ? state.invoiceZip : state.envelopeZip;
        const info = which === 0 ? elInfo0 : elInfo1;
        const input = which === 0 ? elInp0 : elInp1;

        setInvalid(input, false);
        if (info) info.innerHTML = '';

        if (!file) return;

        let okFile = true;
        const msgs = [];

        if (!isZip(file)) { okFile = false; msgs.push('Yalnızca .zip yükleyebilirsiniz.'); }
        if (!isSizeOk(file)) { okFile = false; msgs.push(`Dosya boyutu ${MAX_MB} MB’ı geçemez.`); }

        if (!okFile) {
            setInvalid(input, true);
            if (info) info.innerHTML = `<span style="color:#e74c3c">${msgs.join(' ')}</span>`;
            return;
        }

        if (info) info.innerHTML = `Seçilen: <b>${file.name}</b> (${bytesToSize(file.size)})`;
    }

    function bindDropzone(dzEl, inputEl, which) {
        if (!dzEl || !inputEl) return;

        dzEl.addEventListener('click', () => inputEl.click());

        dzEl.addEventListener('dragover', (e) => { e.preventDefault(); dzEl.classList.add('dragover'); });
        dzEl.addEventListener('dragenter', (e) => { e.preventDefault(); dzEl.classList.add('dragover'); });
        dzEl.addEventListener('dragleave', (e) => { e.preventDefault(); dzEl.classList.remove('dragover'); });
        dzEl.addEventListener('drop', (e) => {
            e.preventDefault();
            dzEl.classList.remove('dragover');

            const dt = e.dataTransfer;
            if (!dt?.files?.length) return;

            const f = dt.files[0];
            if (which === 0) state.invoiceZip = f;
            else state.envelopeZip = f;

            // input.files set etmeye güvenmeyelim (tarayıcı kısıtları olabiliyor)
            inputEl.value = '';
            renderInfo(which);
        });

        inputEl.addEventListener('change', () => {
            const f = inputEl.files && inputEl.files[0] ? inputEl.files[0] : null;
            if (which === 0) state.invoiceZip = f;
            else state.envelopeZip = f;
            renderInfo(which);
        });
    }

    function validateAll() {
        // dosya
        const hasInv = !!state.invoiceZip;
        const hasEnv = !!state.envelopeZip;

        if (!hasInv && !hasEnv) {
            setInvalid(elInp0, true);
            setInvalid(elInp1, true);
            err('En az bir ZIP dosyası seçmelisiniz (Fatura veya Zarf).');
            return false;
        }

        // dosya validasyonu
        if (hasInv && (!isZip(state.invoiceZip) || !isSizeOk(state.invoiceZip))) {
            setInvalid(elInp0, true);
            err('Fatura ZIP dosyası hatalı.');
            return false;
        }
        if (hasEnv && (!isZip(state.envelopeZip) || !isSizeOk(state.envelopeZip))) {
            setInvalid(elInp1, true);
            err('Zarf ZIP dosyası hatalı.');
            return false;
        }

        // select zorunlu
        if (!elIntegrator.value) {
            setInvalid(elIntegrator, true);
            err('Lütfen Entegrator seçiniz.');
            return false;
        }
        setInvalid(elIntegrator, false);

        if (!elTip.value) {
            setInvalid(elTip, true);
            err('Lütfen Fatura Tipi seçiniz.');
            return false;
        }
        setInvalid(elTip, false);

        return true;
    }

    function getCurrentUserIdForGib() {
        let userId = 0;
        const hdn = document.getElementById('hdnUserId');
        if (hdn?.value) {
            const p = parseInt(hdn.value, 10);
            if (!isNaN(p) && p > 0) userId = p;
        }
        if (!userId && typeof window.currentUserId === 'number' && window.currentUserId > 0) userId = window.currentUserId;

        if (!userId) {
            try {
                const stored =
                    sessionStorage.getItem('CurrentUserId') ||
                    sessionStorage.getItem('currentUserId') ||
                    sessionStorage.getItem('UserId');
                if (stored) {
                    const p = parseInt(stored, 10);
                    if (!isNaN(p) && p > 0) userId = p;
                }
            } catch { }
        }
        return userId || null;
    }

    async function submitToTurkcellApi() {
        const userId = getCurrentUserIdForGib();

        const fd = new FormData();
        // View'deki name'lerle aynı gönderelim (files[0], files[1])
        if (state.invoiceZip) fd.append('files[0]', state.invoiceZip, state.invoiceZip.name);
        if (state.envelopeZip) fd.append('files[1]', state.envelopeZip, state.envelopeZip.name);

        fd.append('entegrator', elIntegrator.value);
        fd.append('tip', elTip.value);

        // ekstra: backend isterse
        if (userId) fd.append('userId', String(userId));

        // API çağrısı
        const resp = await EinvoiceTransferApi.upload(userId ? { userId } : null, fd);

        // resp formatı sende farklı olabilir; güvenli mesaj
        const msg = resp?.message || resp?.detail || resp?.title || 'Yükleme başarılı.';
        ok(msg);
        return resp;
    }

    function setUploading(on) {
        if (!elBtnUpload) return;
        if (on) {
            elBtnUpload.disabled = true;
            elBtnUpload.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Yükleniyor...';
        } else {
            elBtnUpload.disabled = false;
            elBtnUpload.innerHTML = 'YÜKLE <span class="btn-label"><i class="glyphicon glyphicon-upload"></i></span>';
        }
    }

    function clearAll() {
        state.invoiceZip = null;
        state.envelopeZip = null;

        if (elInp0) { elInp0.value = ''; setInvalid(elInp0, false); }
        if (elInp1) { elInp1.value = ''; setInvalid(elInp1, false); }

        if (elInfo0) elInfo0.innerHTML = '';
        if (elInfo1) elInfo1.innerHTML = '';

        if (elIntegrator) { elIntegrator.value = ''; setInvalid(elIntegrator, false); }
        if (elTip) { elTip.value = ''; setInvalid(elTip, false); }

        if (elLbl) elLbl.textContent = '';
        ok('Form temizlendi.');
    }

    function init() {
        if (!elForm) return;

        bindDropzone(elDz0, elInp0, 0);
        bindDropzone(elDz1, elInp1, 1);

        elIntegrator?.addEventListener('change', () => setInvalid(elIntegrator, false));
        elTip?.addEventListener('change', () => setInvalid(elTip, false));

        elBtnClear?.addEventListener('click', (e) => { e.preventDefault(); clearAll(); });

        // Enter -> Upload
        elForm.addEventListener('keypress', (e) => {
            const tag = (e.target?.tagName || '').toLowerCase();
            if (e.key === 'Enter' && (tag === 'input' || tag === 'select')) {
                e.preventDefault();
                elBtnUpload?.click();
            }
        });

        // Submit: Turkcell API’ye gönder
        elForm.addEventListener('submit', async (e) => {
            if (elLbl) elLbl.textContent = '';
            if (!validateAll()) { e.preventDefault(); return; }

            e.preventDefault();

            try {
                setUploading(true);
                await submitToTurkcellApi();
                // istersen başarıdan sonra temizle:
                // clearAll();
            } catch (ex) {
                console.error('[UploadTransferInvoice] upload error:', ex);
                err(ex?.message || 'Yükleme sırasında hata oluştu.');
            } finally {
                setUploading(false);
            }
        });
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();

})();
