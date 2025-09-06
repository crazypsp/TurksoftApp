/*! ts-loader.js — ultra-minimal, deterministic full-screen loader */
(function () {
  class BlockingLoader {
    constructor() {
      this.overlay = null;
      this._wheelHandler = null;
    }

    _ensureStyle() {
      if (document.getElementById('ts-blocker-style')) return;
      const css = `
      #ts-blocker{
        position:fixed; inset:0; z-index:999999;
        display:grid; place-items:center;
        background:rgba(0,0,0,.35); backdrop-filter:blur(2px);
        pointer-events:auto; /* görünürken tüm tıklamaları absorbe et */
      }
      #ts-blocker .ts-card{
        width:min(520px,92vw); background:#fff;
        border-radius:14px; padding:18px 20px 16px;
        box-shadow:0 14px 50px rgba(0,0,0,.22); outline:none
      }
      #ts-blocker .ts-title{
        font:600 16px/1.2 system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;
        color:#111; margin:0 0 12px
      }
      #ts-blocker .ts-sub{
        font:400 13px/1.4 system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;
        color:#555; margin:0 0 12px
      }
      #ts-blocker .ts-bar{height:16px;background:#f2f2f2;border-radius:999px;overflow:hidden;box-shadow:inset 0 1px 2px rgba(0,0,0,.06)}
      #ts-blocker .ts-fill{height:100%;width:40%;
        background:repeating-linear-gradient(45deg,#4f9cff,#4f9cff 10px,#6aafff 10px,#6aafff 20px);
        animation:ts-stripes 1s linear infinite; border-radius:999px}
      @keyframes ts-stripes{0%{background-position:0 0}100%{background-position:40px 0}}
      #ts-blocker .ts-footer{display:flex;justify-content:flex-end;align-items:center;margin-top:10px}
      #ts-blocker .ts-dot{width:6px;height:6px;border-radius:50%;background:#4f9cff;animation:ts-breathe 1.2s ease-in-out infinite;margin-left:8px}
      @keyframes ts-breathe{0%{transform:scale(1);opacity:.8}50%{transform:scale(1.35);opacity:1}100%{transform:scale(1);opacity:.8}}
      `;
      const style = document.createElement('style');
      style.id = 'ts-blocker-style';
      style.textContent = css;
      document.head.appendChild(style);
    }

    _createOverlay() {
      this._ensureStyle();
      const o = document.createElement('div');
      o.id = 'ts-blocker';
      o.setAttribute('aria-busy', 'true');
      o.setAttribute('aria-modal', 'true');
      o.setAttribute('role', 'dialog');
      o.innerHTML = `
        <div class="ts-card" tabindex="0">
          <h3 class="ts-title" id="ts-title">İşlem yapılıyor…</h3>
          <p class="ts-sub"   id="ts-sub">Lütfen bitene kadar bekleyin.</p>
          <div class="ts-bar"><div class="ts-fill"></div></div>
          <div class="ts-footer"><span class="ts-dot"></span></div>
        </div>
      `;

      // Kart dışına gelen tüm tıklamaları yut (alt katmana geçmesin)
      o.addEventListener('click', (e) => {
        if (!e.target.closest('.ts-card')) { e.preventDefault(); e.stopPropagation(); }
      }, true);

      // Yükleme sırasında sayfa kaymasın istiyorsan bu iki handler açık kalsın;
      // istemiyorsan yorum satırına al:
      const wheelBlock = (e) => { e.preventDefault(); e.stopPropagation(); };
      o.addEventListener('wheel', wheelBlock, { passive: false, capture: true });
      o.addEventListener('touchmove', wheelBlock, { passive: false, capture: true });
      this._wheelHandler = wheelBlock;

      this.overlay = o;
      return o;
    }

    show(title = 'İşlem yapılıyor…', subtitle = 'Lütfen bitene kadar bekleyin.') {
      // Her çağrıda tek overlay: tekrar show() edilirse sadece metni günceller.
      if (!this.overlay) this._createOverlay();
      const o = this.overlay;
      o.querySelector('#ts-title').textContent = title;
      o.querySelector('#ts-sub').textContent = subtitle;

      if (!document.body.contains(o)) {
        document.body.appendChild(o);
        // odağı karta al (focus trap yok; sadece başlangıç odağı)
        const card = o.querySelector('.ts-card');
        setTimeout(() => card && card.focus({ preventScroll: true }), 0);
      }
    }

    hide() {
      const o = this.overlay;
      if (!o) return;

      // Dinleyicileri sök
      if (this._wheelHandler) {
        try { o.removeEventListener('wheel', this._wheelHandler, true); } catch { }
        try { o.removeEventListener('touchmove', this._wheelHandler, true); } catch { }
        this._wheelHandler = null;
      }

      // DOM’dan kesin çıkar
      if (o.parentNode) o.parentNode.removeChild(o);
      this.overlay = null; // bir sonraki show’da sıfırdan kur
    }

    async run(op, { title = 'İşlem yapılıyor…', subtitle = 'Lütfen bitene kadar bekleyin.' } = {}) {
      this.show(title, subtitle);
      try {
        const p = (typeof op === 'function') ? op() : op;
        return await p;
      } finally {
        this.hide(); // resolve da reject de olsa kesin kapanır
      }
    }
  }

  window.TSLoader = new BlockingLoader();
})();
