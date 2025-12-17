// wwwroot/apps/einvoice/GibPortalCancelledOrObjected.js
import { EinvoiceGibPortalApi } from '../Base/turkcellEfaturaApi.js';

(function () {
    'use strict';

    // ===== DOM =====
    const elUUID = document.getElementById('GibPortalCancelUUID');
    const elHelp = document.getElementById('uuidHelp');
    const elAppType = document.getElementById('GibCancelAppType');
    const elChk = document.getElementById('uyariyiOkudumGibIptalchk');

    const elBtnClose = document.getElementById('btnClose');
    const elBtnSet = document.getElementById('iptalEtGibBtn');
    const elBtnUnset = document.getElementById('iptalKaldirGibBtn');

    // ===== Notify =====
    function ok(m) {
        if (window.toastr?.success) toastr.success(m);
        else alert(m);
    }
    function err(m) {
        if (window.toastr?.error) toastr.error(m);
        else alert(m);
    }

    // ===== UserId (opsiyonel) =====
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

    // ===== UUID doğrulama =====
    const uuidHyphenRe = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;
    const uuidPlainRe = /^[0-9a-fA-F]{32}$/;

    function onlyHex(s) {
        return String(s || '').replace(/[^0-9a-fA-F]/g, '');
    }

    function formatUUIDIfPlain(v) {
        const s = onlyHex(v);
        if (uuidPlainRe.test(s)) {
            return (s.substr(0, 8) + '-' + s.substr(8, 4) + '-' + s.substr(12, 4) + '-' + s.substr(16, 4) + '-' + s.substr(20));
        }
        return v;
    }

    function isValidUUID(v) {
        return uuidHyphenRe.test(v) || uuidPlainRe.test(onlyHex(v));
    }

    function setInvalid(el, v) {
        if (!el) return;
        if (v) el.classList.add('is-invalid');
        else el.classList.remove('is-invalid');
    }

    function getState() {
        const raw = (elUUID?.value || '').trim();
        const uuidFormatted = formatUUIDIfPlain(raw);
        const appType = (elAppType?.value || '').trim(); // 1: gelen, 2: giden
        const chk = !!elChk?.checked;

        const validUuid = raw.length === 0 ? false : isValidUUID(uuidFormatted);
        setInvalid(elUUID, raw.length > 0 && !validUuid);

        if (elHelp) {
            elHelp.textContent = validUuid || raw.length === 0 ? '' : 'Geçerli bir UUID/ETTN girin.';
        }

        const canSubmit = validUuid && !!appType && chk;
        if (elBtnSet) elBtnSet.disabled = !canSubmit;
        if (elBtnUnset) elBtnUnset.disabled = !canSubmit;

        return {
            canSubmit,
            uuid: uuidFormatted,
            appType: appType ? parseInt(appType, 10) : null
        };
    }

    function resetForm() {
        if (elUUID) elUUID.value = '';
        if (elAppType) elAppType.value = '';
        if (elChk) elChk.checked = false;
        if (elHelp) elHelp.textContent = '';
        setInvalid(elUUID, false);
        getState();
    }

    function setBusy(on, whichFlag) {
        const btns = [elBtnSet, elBtnUnset].filter(Boolean);
        btns.forEach(b => b.disabled = true);

        const btn = whichFlag === 1 ? elBtnSet : elBtnUnset;
        if (!btn) return () => { };

        const oldHtml = btn.innerHTML;
        btn.innerHTML = on ? '<i class="fa fa-spinner fa-spin"></i> İşleniyor...' : oldHtml;

        return () => {
            btn.innerHTML = oldHtml;
            getState();
        };
    }

    // ===== Global fonksiyonlar (View onclick’leri bozulmasın) =====
    window.uyariyiOkudumClick = function () {
        getState();
    };

    window.invoiceCancelGibSend = async function (flag) {
        const st = getState();
        if (!st.canSubmit) {
            err('Zorunlu alanları doldurun (UUID + Belge Türü + Uyarıyı Okudum).');
            return;
        }

        // Blur gibi davran: 32 hane ise tirele + uppercase
        const uuidFinal = formatUUIDIfPlain(st.uuid).toUpperCase();
        if (elUUID) elUUID.value = uuidFinal;

        const userId = getCurrentUserIdForGib();

        const dto = {
            uuid: uuidFinal,
            appType: st.appType,           // 1:Gelen  2:Giden
            flag: flag === 1,              // true: işaretle, false: kaldır
            userId: userId || undefined    // backend kullanıyorsa
        };

        const restore = setBusy(true, flag);

        try {
            const res = await EinvoiceGibPortalApi.setCancelledOrObjectedFlag(dto);

            const msg =
                res?.message ||
                (flag === 1
                    ? 'Belge GİB Portalından iptal/itiraz edildi olarak işaretlendi.'
                    : 'Belgenin GİB Portalı iptal/itiraz işareti kaldırıldı.');

            ok(msg);
            resetForm();
        } catch (ex) {
            console.error('[GibPortalCancelledOrObjected] API error:', ex);
            err(ex?.message || 'İşlem sırasında hata oluştu.');
        } finally {
            restore();
        }
    };

    // ===== Events =====
    function init() {
        if (!elUUID || !elAppType || !elChk) return;

        elUUID.addEventListener('input', getState);
        elUUID.addEventListener('blur', () => {
            const v = (elUUID.value || '').trim();
            const f = formatUUIDIfPlain(v);
            elUUID.value = String(f || '').toUpperCase();
            getState();
        });

        elAppType.addEventListener('change', getState);
        elChk.addEventListener('change', getState);

        // Enter ile (uygunsa) işaretle
        const container = document.getElementById('btnAramaYapEnter');
        container?.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                if (elBtnSet && !elBtnSet.disabled) elBtnSet.click();
            }
        });

        elBtnClose?.addEventListener('click', () => {
            // modal içindeyse kapat, değilse geri
            const modal = elBtnClose.closest('.modal');
            if (modal && window.jQuery) {
                window.jQuery(modal).modal('hide');
            } else {
                window.history.back();
            }
        });

        // İlk durum
        getState();
        elUUID.focus();
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();
})();
