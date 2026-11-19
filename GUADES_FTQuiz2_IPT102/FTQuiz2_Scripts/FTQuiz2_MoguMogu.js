// Mogu Mogu shell behavior (Phase 1; keyboard polish in redesign Phase 01). Loaded after FTQuiz2_Portfolio.js in the same bundle.
// Handles the header (mobile menu, Admin dropdown, one-menu-at-a-time, scroll shadow, Esc / focus handling) and product-modal focus handling (redesign Phase 05).
document.addEventListener("DOMContentLoaded", function () {

    var header = document.getElementById("mmHeader");
    var nav = document.getElementById("mmNav");
    var navToggle = document.getElementById("mmNavToggle");
    var adminMenu = document.getElementById("adminMenu");
    var accountMenu = document.getElementById("accountMenu");

    // ---- Mobile menu ----
    function setNav(open) {
        if (!nav || !navToggle) { return; }
        nav.classList.toggle("open", open);
        navToggle.setAttribute("aria-expanded", open ? "true" : "false");
        navToggle.setAttribute("aria-label", open ? "Close menu" : "Open menu");
    }

    if (nav && navToggle) {
        navToggle.addEventListener("click", function () {
            setNav(!nav.classList.contains("open"));
        });

        // Choosing a link or the Contact button closes the mobile menu.
        nav.querySelectorAll("a, button").forEach(function (control) {
            control.addEventListener("click", function () { setNav(false); });
        });

        document.addEventListener("click", function (e) {
            if (header && !header.contains(e.target)) { setNav(false); }
        });

        window.addEventListener("resize", function () {
            if (window.innerWidth > 900) { setNav(false); }
        });
    }

    // ---- Admin dropdown: close on outside click / Esc; never open together with the account menu ----
    if (adminMenu) {
        document.addEventListener("click", function (e) {
            if (!adminMenu.contains(e.target)) { adminMenu.removeAttribute("open"); }
        });

        adminMenu.addEventListener("toggle", function () {
            if (adminMenu.open && accountMenu) { accountMenu.removeAttribute("open"); }
        });
    }

    if (accountMenu) {
        accountMenu.addEventListener("toggle", function () {
            if (accountMenu.open && adminMenu) { adminMenu.removeAttribute("open"); }
        });
    }

    // Esc closes whatever is open and hands focus back to the control that opened it.
    document.addEventListener("keydown", function (e) {
        if (e.key !== "Escape") { return; }

        var navWasOpen = nav && nav.classList.contains("open");
        var adminWasOpen = adminMenu && adminMenu.hasAttribute("open");
        var accountWasOpen = accountMenu && accountMenu.hasAttribute("open");

        if (adminMenu) { adminMenu.removeAttribute("open"); }
        if (accountMenu) { accountMenu.removeAttribute("open"); }
        setNav(false);

        if (accountWasOpen) { focusSummary(accountMenu); }
        else if (adminWasOpen) { focusSummary(adminMenu); }
        else if (navWasOpen && navToggle) { navToggle.focus(); }
    });

    function focusSummary(menu) {
        var summary = menu.querySelector("summary");
        if (summary) { summary.focus(); }
    }

    // Keyboard users tabbing past the last item close the dropdown instead of leaving it hanging open.
    [adminMenu, accountMenu].forEach(function (menu) {
        if (!menu) { return; }
        menu.addEventListener("focusout", function (e) {
            if (menu.hasAttribute("open") && e.relatedTarget && !menu.contains(e.relatedTarget)) {
                menu.removeAttribute("open");
            }
        });
    });

    // ---- Stronger header shadow once the page scrolls ----
    function onScroll() {
        if (header) { header.classList.toggle("is-scrolled", window.scrollY > 4); }
    }
    window.addEventListener("scroll", onScroll, { passive: true });
    onScroll();

    // ---- Product modal focus handling (redesign Phase 05) ----
    // FTQuiz2_Portfolio.js opens/closes the dialog (adds/removes .open). This only manages focus:
    // focus goes to the close button on open, Tab stays inside the dialog, and focus returns to the trigger on close.
    var modalTrigger = null;
    var focusableSelector = 'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

    document.querySelectorAll("[data-modal]").forEach(function (trigger) {
        trigger.addEventListener("click", function () {
            var overlay = document.getElementById(trigger.getAttribute("data-modal"));
            var closeBtn = overlay ? overlay.querySelector("[data-close]") : null;
            if (!closeBtn) { return; }
            modalTrigger = trigger;
            closeBtn.focus();
            if (document.activeElement !== closeBtn) {
                window.requestAnimationFrame(function () { closeBtn.focus(); });
            }
        });
    });

    if (window.MutationObserver) {
        var overlayObserver = new MutationObserver(function (records) {
            records.forEach(function (record) {
                var overlay = record.target;
                var wasOpen = (record.oldValue || "").split(/\s+/).indexOf("open") !== -1;
                if (wasOpen && !overlay.classList.contains("open") && modalTrigger) {
                    modalTrigger.focus();
                    modalTrigger = null;
                }
            });
        });
        document.querySelectorAll(".modal-overlay").forEach(function (overlay) {
            overlayObserver.observe(overlay, { attributes: true, attributeFilter: ["class"], attributeOldValue: true });
        });
    }

    document.addEventListener("keydown", function (e) {
        if (e.key !== "Tab") { return; }
        var overlay = document.querySelector(".modal-overlay.open");
        if (!overlay) { return; }
        var items = Array.prototype.slice.call(overlay.querySelectorAll(focusableSelector));
        if (items.length === 0) { return; }
        var first = items[0];
        var last = items[items.length - 1];
        if (!overlay.contains(document.activeElement)) {
            e.preventDefault();
            first.focus();
        } else if (e.shiftKey && document.activeElement === first) {
            e.preventDefault();
            last.focus();
        } else if (!e.shiftKey && document.activeElement === last) {
            e.preventDefault();
            first.focus();
        }
    });
});
