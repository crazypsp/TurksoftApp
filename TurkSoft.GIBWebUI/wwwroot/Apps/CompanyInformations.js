
; (function (global, $) {
    "use strict";

    /***********************************************************
     *  DEBUG (çok önemli)
     ***********************************************************/
    console.log("[CompanyInformations] ✅ script loaded");

    /***********************************************************
     *  UTILS
     ***********************************************************/
    const Utils = {
        trim: (v) => (v === undefined || v === null) ? "" : String(v).trim(),
        firstNonEmpty: (...vals) => {
            for (const v of vals) {
                const s = (v === undefined || v === null) ? "" : String(v).trim();
                if (s) return s;
            }
            return "";
        },
        safeJsonParse: (txt) => {
            try { return JSON.parse(txt); } catch { return null; }
        },
        setVal: ($el, val, force) => {
            if (!$el || !$el.length) return false;
            const v = Utils.trim(val);
            if (!v) return false;

            if (force) {
                $el.val(v).trigger("input").trigger("change");
                return true;
            }

            const cur = Utils.trim($el.val());
            if (!cur) {
                $el.val(v).trigger("input").trigger("change");
                return true;
            }
            return false;
        },
        normalizeTrUpper: (s) => {
            return Utils.trim(s)
                .toUpperCase()
                .replace(/İ/g, "I")
                .replace(/İ/g, "I")
                .replace(/Ş/g, "S")
                .replace(/Ğ/g, "G")
                .replace(/Ü/g, "U")
                .replace(/Ö/g, "O")
                .replace(/Ç/g, "C");
        }
    };

    /***********************************************************
     *  SESSION: Firma (kesin buradan okunuyor)
     ***********************************************************/
    function getFirmaFromSession() {
        try {
            const firmaJson = sessionStorage.getItem("Firma"); // <-- SENİN İSTEDİĞİN
            if (!firmaJson) return null;

            const firma = Utils.safeJsonParse(firmaJson);
            if (!firma || typeof firma !== "object") return null;

            return firma;
        } catch (e) {
            console.warn("[CompanyInformations] sessionStorage erişim hatası:", e);
            return null;
        }
    }

    /***********************************************************
     *  SELECT helper: option yoksa ekle + set et
     ***********************************************************/
    function setSelectSmart($select, textOrValue, force) {
        if (!$select || !$select.length) return false;

        const raw = Utils.trim(textOrValue);
        if (!raw) return false;

        const norm = (s) => Utils.normalizeTrUpper(s)
            .replace(/[.\-_,/()]/g, " ")
            .replace(/\s+/g, " ")
            .trim();

        const want = norm(raw);

        // 1) value ile direkt dene
        if ($select.find(`option[value="${raw}"]`).length) {
            if (force || !Utils.trim($select.val())) {
                $select.val(raw).trigger("change");
                if ($.fn.select2) $select.trigger("change.select2");
            }
            return true;
        }

        // 2) text/value benzerliği ile en iyi eşleşmeyi bul
        let bestVal = "";
        let bestScore = -1;

        $select.find("option").each(function () {
            const $opt = $(this);
            const val = Utils.trim($opt.attr("value") ?? $opt.val());
            const txt = Utils.trim($opt.text());
            const key = norm(val || txt);
            if (!key) return;

            if (key === want) {
                bestVal = val;
                bestScore = 9999;
                return false;
            }

            let score = 0;
            if (key.includes(want)) score = want.length;
            else if (want.includes(key)) score = key.length;

            if (score > bestScore) {
                bestScore = score;
                bestVal = val;
            }
        });

        if (bestVal) {
            if (force || !Utils.trim($select.val())) {
                $select.val(bestVal).trigger("change");
                if ($.fn.select2) $select.trigger("change.select2");
            }
            return true;
        }

        // 3) hiç yoksa ekle
        $select.append(new Option(raw, raw));
        $select.val(raw).trigger("change");
        if ($.fn.select2) $select.trigger("change.select2");
        return true;
    }

    /***********************************************************
     *  API resolve (import yok -> global'dan bul)
     ***********************************************************/
    function resolveApis() {
        // projene göre bunlar window’da olabilir
        const CompanyInformationsApi =
            global.CompanyInformationsApi ||
            (global.Entities && global.Entities.CompanyInformationsApi) ||
            null;

        const ContractApi =
            global.ContractApi ||
            (global.Entities && global.Entities.ContractApi) ||
            null;

        const BranchApi =
            global.BranchApi ||
            (global.Entities && global.Entities.BranchApi) ||
            null;

        return { CompanyInformationsApi, ContractApi, BranchApi };
    }

    /***********************************************************
     *  APP
     ***********************************************************/
    const App = {
        state: {
            appliedSessionOnce: false,
            currentCompanyId: null
        },
        els: {},

        cacheElements: function () {
            const e = this.els;

            e.formEl = $("#frmCompany");

            e.fmId = $("#cmpId");
            e.fmVkn = $("#vknTckn");
            e.fmName = $("#musteriAdi");
            e.fmTitle = $("#unvan");
            e.fmKep = $("#kepAdresi");

            e.fmPersonName = $("#ad");
            e.fmPersonSurname = $("#soyad");
            e.fmKurumTuru = $("#kurumTuru");
            e.fmCorporateMail = $("#kurumsalEposta");

            e.fmTaxOfficeCity = $("#vergiDairesiIl");
            e.fmTaxOffice = $("#vergiDairesi");
            e.fmCustomerRep = $("#musteriTemsilcisi");

            e.fmRespTckn = $("#sorumluTckn");
            e.fmRespAd = $("#sorumluAd");
            e.fmRespSoyad = $("#sorumluSoyad");
            e.fmRespPhone = $("#sorumluCepTel");
            e.fmRespEmail = $("#sorumluEPosta");

            e.fmRegName = $("#kaydedenAd");
            e.fmRegSurname = $("#kaydedenSoyad");
            e.fmRegPhone = $("#kaydedenTel");
        },

        formReady: function () {
            // kritik input var mı?
            return $("#vknTckn").length > 0 && $("#unvan").length > 0;
        },

        applySessionFirmaToForm: function (force) {
            this.cacheElements();
            const e = this.els;

            const firma = getFirmaFromSession();
            if (!firma) {
                console.log("[CompanyInformations] sessionStorage.Firma yok / boş.");
                return false;
            }

            console.log("[CompanyInformations] ✅ session Firma bulundu:", firma);

            // Id
            if (firma.id != null) {
                const idNum = Number(firma.id);
                if (!isNaN(idNum)) {
                    this.state.currentCompanyId = idNum;
                    Utils.setVal(e.fmId, String(idNum), true);
                }
            }

            // Session JSON alanları -> form
            Utils.setVal(e.fmVkn, firma.taxNo, force);
            Utils.setVal(e.fmTitle, firma.title, force);
            Utils.setVal(e.fmName, Utils.firstNonEmpty(firma.customerName, firma.title), force);
            Utils.setVal(e.fmKep, firma.kepAddress, force);

            Utils.setVal(e.fmPersonName, firma.personalFirstName, force);
            Utils.setVal(e.fmPersonSurname, firma.personalLastName, force);

            // kurum türü select
            if (firma.institutionType !== undefined && firma.institutionType !== null) {
                const val = String(firma.institutionType);
                if (force || !Utils.trim(e.fmKurumTuru.val())) {
                    e.fmKurumTuru.val(val).trigger("change");
                    if ($.fn.select2) e.fmKurumTuru.trigger("change.select2");
                }
            }

            Utils.setVal(
                e.fmCorporateMail,
                Utils.firstNonEmpty(firma.corporateEmail, firma.email),
                force
            );

            Utils.setVal(e.fmCustomerRep, firma.customerRepresentative, force);

            // Vergi dairesi il / daire
            const taxCity = Utils.firstNonEmpty(firma.taxOfficeProvince, firma.city);
            if (taxCity) setSelectSmart(e.fmTaxOfficeCity, taxCity, force);

            // il değişimi sonrası option değişebilir → tick sonra set
            if (firma.taxOffice) {
                setTimeout(() => {
                    setSelectSmart(e.fmTaxOffice, firma.taxOffice, force);
                }, 50);
            }

            // Sorumlu
            Utils.setVal(e.fmRespTckn, firma.responsibleTckn, force);
            Utils.setVal(e.fmRespAd, firma.responsibleFirstName, force);
            Utils.setVal(e.fmRespSoyad, firma.responsibleLastName, force);
            Utils.setVal(e.fmRespPhone, firma.responsibleMobilePhone, force);
            Utils.setVal(e.fmRespEmail, firma.responsibleEmail, force);

            // Kaydı alan
            Utils.setVal(e.fmRegName, firma.createdByPersonFirstName, force);
            Utils.setVal(e.fmRegSurname, firma.createdByPersonLastName, force);
            Utils.setVal(e.fmRegPhone, firma.createdByPersonMobilePhone, force);

            console.log("[CompanyInformations] ✅ Session forma basıldı =>", {
                vkn: e.fmVkn.val(),
                unvan: e.fmTitle.val(),
                musteriAdi: e.fmName.val()
            });

            this.state.appliedSessionOnce = true;
            return true;
        },

        applySessionWithRetry: function () {
            let tries = 0;
            const maxTry = 20; // 20 * 200ms = 4sn

            const tick = () => {
                tries++;

                if (!this.formReady()) {
                    if (tries < maxTry) return setTimeout(tick, 200);
                    console.warn("[CompanyInformations] Form elementleri bulunamadı. (id'ler uyuşmuyor olabilir)");
                    return;
                }

                const ok = this.applySessionFirmaToForm(true);
                if (!ok) {
                    if (tries < maxTry) return setTimeout(tick, 200);
                    console.warn("[CompanyInformations] Session bulundu ama parse/format sorunu olabilir.");
                    return;
                }
            };

            tick();
        },

        observeDom: function () {
            // Form sonradan render oluyorsa yakala
            const self = this;
            const obs = new MutationObserver(function () {
                if (self.formReady() && !self.state.appliedSessionOnce) {
                    console.log("[CompanyInformations] DOM geldi -> session tekrar basılıyor");
                    self.applySessionFirmaToForm(true);
                }
            });

            obs.observe(document.documentElement, { childList: true, subtree: true });
        },

        loadFromApiIfExists: async function () {
            const { CompanyInformationsApi } = resolveApis();
            if (!CompanyInformationsApi || typeof CompanyInformationsApi.list !== "function") {
                console.warn("[CompanyInformations] API yok (CompanyInformationsApi.list bulunamadı). Session ile devam.");
                return;
            }

            try {
                const resp = await CompanyInformationsApi.list();
                const arr = Array.isArray(resp) ? resp : (resp && (resp.data || resp.items)) || [];
                if (!arr || !arr.length) {
                    console.log("[CompanyInformations] API boş döndü. Session zaten basıldı.");
                    return;
                }

                // API doluysa sadece dolu alanlarla override edebilirsin istersen
                console.log("[CompanyInformations] API geldi, ama session öncelikli. (Override yok)");
            } catch (e) {
                console.warn("[CompanyInformations] API hata verdi. Session ile devam.", e);
            }
        },

        init: function () {
            // select2 varsa
            if ($.fn.select2) {
                $(".select2").select2({ width: "100%" });
            }

            // en kritik: session’ı kesin uygula
            this.observeDom();
            this.applySessionWithRetry();

            // api varsa sonra dene
            this.loadFromApiIfExists();
        }
    };

    global.CompanyInformationsApp = App;

})(window, jQuery);

// Invoice.js gibi sayfa yüklenince başlat
$(function () {
    if (window.CompanyInformationsApp && typeof window.CompanyInformationsApp.init === "function") {
        window.CompanyInformationsApp.init();
    }
});
