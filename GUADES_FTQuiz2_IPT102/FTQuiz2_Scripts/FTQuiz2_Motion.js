/* ============================================================================
   FTQuiz2_Motion.js  —  motion + interaction layer (v2)
   ----------------------------------------------------------------------------
   Effects are adapted from the BrewedOps Motion Library patterns:
     Navigation › Glass on scroll / Hide on scroll
     Scroll patterns › Scroll triggered reveal, Staggered list entrance,
                       Scroll progress bar
     Text › Marquee ticker (CSS driven, duplicated here)
     Buttons › Magnetic button, Button shine sweep (CSS)
     Cards › 3D tilt
     Stats › Count up
     Forms › Password strength, Inline validation, Submit to success
   All effects are opt-out via prefers-reduced-motion and never block the page.
   ========================================================================== */
(function () {
    "use strict";

    var reduceMotion = window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    var supportsIO = "IntersectionObserver" in window;

    function ready(fn) {
        if (document.readyState === "loading") {
            document.addEventListener("DOMContentLoaded", fn);
        } else {
            fn();
        }
    }

    /* ---------------------------------------------------- header + progress */
    function initChrome() {
        var header = document.getElementById("mmHeader");
        var progress = document.createElement("div");
        progress.className = "mg-scroll-progress";
        progress.setAttribute("aria-hidden", "true");
        document.body.appendChild(progress);

        var ticking = false;
        function onScroll() {
            if (ticking) { return; }
            ticking = true;
            window.requestAnimationFrame(function () {
                var y = window.pageYOffset || document.documentElement.scrollTop;
                if (header) { header.classList.toggle("is-scrolled", y > 12); }

                var doc = document.documentElement;
                var max = (doc.scrollHeight - window.innerHeight) || 1;
                progress.style.transform = "scaleX(" + Math.min(y / max, 1) + ")";
                ticking = false;
            });
        }
        window.addEventListener("scroll", onScroll, { passive: true });
        onScroll();

        /* Mobile nav toggle: keeps aria-expanded honest. */
        var toggle = document.getElementById("mmNavToggle");
        var nav = document.getElementById("mmNav");
        if (toggle && nav) {
            toggle.addEventListener("click", function () {
                var open = nav.classList.toggle("open");
                toggle.setAttribute("aria-expanded", open ? "true" : "false");
                toggle.setAttribute("aria-label", open ? "Close menu" : "Open menu");
            });
            nav.addEventListener("click", function (e) {
                if (e.target.closest("a")) {
                    nav.classList.remove("open");
                    toggle.setAttribute("aria-expanded", "false");
                }
            });
        }

        /* Close <details> menus when clicking outside them. */
        document.addEventListener("click", function (e) {
            document.querySelectorAll("details.mm-menu[open], details.account-menu[open]").forEach(function (d) {
                if (!d.contains(e.target)) { d.removeAttribute("open"); }
            });
        });

        /* Escape closes any open menu. */
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") {
                document.querySelectorAll("details[open]").forEach(function (d) { d.removeAttribute("open"); });
            }
        });
    }

    /* ------------------------------------------------------------- marquees */
    /* Duplicate each marquee group so the -50% keyframe loops seamlessly. */
    function initMarquees() {
        document.querySelectorAll("[data-mg-marquee]").forEach(function (track) {
            var group = track.querySelector(".mg-ticker__group, .mg-band__group");
            if (!group || track.dataset.mgCloned === "1") { return; }
            track.appendChild(group.cloneNode(true));
            track.dataset.mgCloned = "1";
        });
    }

    /* --------------------------------------------------------- scroll reveal */
    function initReveal() {
        var items = document.querySelectorAll("[data-mg-reveal]");
        if (!items.length) { return; }

        if (reduceMotion || !supportsIO) {
            items.forEach(function (el) { el.classList.add("is-in"); });
            return;
        }

        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) { return; }
                entry.target.classList.add("is-in");
                io.unobserve(entry.target);
            });
        }, { rootMargin: "0px 0px -8% 0px", threshold: 0.08 });

        items.forEach(function (el, i) {
            if (!el.style.getPropertyValue("--mg-delay")) {
                el.style.setProperty("--mg-delay", Math.min(i % 8, 7) * 70 + "ms");
            }
            io.observe(el);
        });
    }

    /* -------------------------------------------------------- magnetic buttons */
    function initMagnetic() {
        if (reduceMotion || window.matchMedia("(hover: none)").matches) { return; }
        document.querySelectorAll(".pill-btn-primary, .mg-admin-nav__link").forEach(function (el) {
            el.addEventListener("mousemove", function (e) {
                var r = el.getBoundingClientRect();
                var x = (e.clientX - r.left - r.width / 2) / r.width;
                var y = (e.clientY - r.top - r.height / 2) / r.height;
                el.style.transform = "translate(" + (x * 5).toFixed(2) + "px," + (y * 4 - 1).toFixed(2) + "px)";
            });
            el.addEventListener("mouseleave", function () { el.style.transform = ""; });
        });
    }

    /* --------------------------------------------------------------- 3D tilt */
    function initTilt() {
        if (reduceMotion || window.matchMedia("(hover: none)").matches) { return; }
        document.querySelectorAll("[data-mg-tilt]").forEach(function (card) {
            card.addEventListener("mousemove", function (e) {
                var r = card.getBoundingClientRect();
                var px = (e.clientX - r.left) / r.width - 0.5;
                var py = (e.clientY - r.top) / r.height - 0.5;
                card.style.transform = "perspective(900px) rotateY(" + (px * 6).toFixed(2) + "deg) rotateX(" + (-py * 6).toFixed(2) + "deg)";
            });
            card.addEventListener("mouseleave", function () { card.style.transform = ""; });
        });
    }

    /* -------------------------------------------------------------- count up */
    function initCounters() {
        var nums = document.querySelectorAll(".stat-value");
        if (!nums.length) { return; }

        function animate(el) {
            var raw = (el.textContent || "").trim();
            var target = parseInt(raw.replace(/[^0-9]/g, ""), 10);
            if (isNaN(target) || target === 0 || reduceMotion) { return; }
            var start = null;
            var dur = 900;
            function step(ts) {
                if (!start) { start = ts; }
                var p = Math.min((ts - start) / dur, 1);
                var eased = 1 - Math.pow(1 - p, 3);
                el.textContent = Math.round(target * eased).toLocaleString();
                if (p < 1) { window.requestAnimationFrame(step); }
            }
            window.requestAnimationFrame(step);
        }

        if (!supportsIO) { nums.forEach(animate); return; }
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) { animate(entry.target); io.unobserve(entry.target); }
            });
        }, { threshold: 0.4 });
        nums.forEach(function (n) { io.observe(n); });
    }

    /* ----------------------------------------------------- password UX (port) */
    /* Ported from GUADES_Quiz_IPT102-main Portfolio.js:
       show/hide toggle, 0-4 strength score, live label.
       Added here: requirement checklist, confirm-match note, caps-lock hint,
       and a submit guard so a too-weak password cannot be posted. */
    function passwordStrengthScore(value) {
        var score = 0;
        if (value.length >= 8) { score++; }
        if (value.length >= 12) { score++; }
        if (/[a-z]/.test(value) && /[A-Z]/.test(value)) { score++; }
        if (/[0-9]/.test(value)) { score++; }
        if (/[^A-Za-z0-9]/.test(value)) { score++; }
        return Math.min(score, 4);
    }

    function initPasswords() {
        var labels = ["Very weak", "Weak", "Fair", "Good", "Strong"];

        /* Show / hide */
        document.querySelectorAll(".password-toggle").forEach(function (btn) {
            var field = btn.closest(".password-field");
            var input = field ? field.querySelector("input") : null;
            if (!input) { return; }
            btn.addEventListener("click", function () {
                var hidden = input.type === "password";
                input.type = hidden ? "text" : "password";
                btn.classList.toggle("is-visible", hidden);
                btn.setAttribute("aria-label", hidden ? "Hide password" : "Show password");
            });
        });

        /* Strength meters */
        document.querySelectorAll("[data-strength-for]").forEach(function (meter) {
            var input = document.getElementById(meter.getAttribute("data-strength-for"));
            var fill = meter.querySelector("[data-strength-fill]");
            var label = meter.querySelector("[data-strength-label]");
            if (!input || !fill || !label) { return; }

            var rules = meter.parentNode.querySelector("[data-pwd-rules]");
            var ruleItems = rules ? rules.querySelectorAll("li[data-rule]") : [];

            function checkRule(name, value) {
                switch (name) {
                    case "length": return value.length >= 8;
                    case "upper": return /[A-Z]/.test(value);
                    case "lower": return /[a-z]/.test(value);
                    case "number": return /[0-9]/.test(value);
                    case "symbol": return /[^A-Za-z0-9]/.test(value);
                    default: return false;
                }
            }

            function update() {
                var value = input.value;
                if (!value) {
                    fill.className = "pwd-strength-fill";
                    fill.style.width = "0%";
                    label.textContent = "";
                    ruleItems.forEach(function (li) { li.classList.remove("is-met"); });
                    return;
                }
                var score = passwordStrengthScore(value);
                fill.className = "pwd-strength-fill pwd-strength-" + score;
                fill.style.width = ((score + 1) / 5 * 100) + "%";
                label.textContent = labels[score];
                ruleItems.forEach(function (li) {
                    li.classList.toggle("is-met", checkRule(li.getAttribute("data-rule"), value));
                });
            }

            input.addEventListener("input", update);
            input.addEventListener("focus", update);
            update();
        });

        /* Confirm-match notes */
        document.querySelectorAll("[data-pwd-match]").forEach(function (note) {
            var confirm = document.getElementById(note.getAttribute("data-pwd-match"));
            var target = document.getElementById(note.getAttribute("data-pwd-target"));
            if (!confirm || !target) { return; }
            function update() {
                if (!confirm.value) { note.textContent = ""; note.className = "pwd-match-note"; return; }
                var ok = confirm.value === target.value;
                note.textContent = ok ? "Passwords match" : "Passwords do not match yet";
                note.className = "pwd-match-note " + (ok ? "is-ok" : "is-bad");
            }
            confirm.addEventListener("input", update);
            target.addEventListener("input", update);
            update();
        });

        /* Caps lock hint */
        document.querySelectorAll(".password-field input").forEach(function (input) {
            var hint = input.parentNode.querySelector("[data-caps-hint]");
            if (!hint) { return; }
            input.addEventListener("keyup", function (e) {
                if (typeof e.getModifierState !== "function") { return; }
                hint.hidden = !e.getModifierState("CapsLock");
            });
        });

        /* Submit guard: only for forms that opt in with data-pwd-guard. */
        document.querySelectorAll("form[data-pwd-guard]").forEach(function (form) {
            form.addEventListener("submit", function (e) {
                var meter = form.querySelector("[data-strength-for]");
                var input = meter ? document.getElementById(meter.getAttribute("data-strength-for")) : null;
                if (!input || !input.value) { return; }
                if (passwordStrengthScore(input.value) < 2) {
                    e.preventDefault();
                    input.focus();
                    var label = meter.querySelector("[data-strength-label]");
                    if (label) { label.textContent = "Too weak — add length, numbers or symbols"; }
                }
            });
        });
    }

    /* ------------------------------------------------------------ form polish */
    function initForms() {
        /* Auto-dismiss success notices after 6s (ported from Quiz 1). */
        document.querySelectorAll(".alert-success").forEach(function (box) {
            window.setTimeout(function () {
                box.style.transition = "opacity .35s, transform .35s";
                box.style.opacity = "0";
                box.style.transform = "translateY(-6px)";
                window.setTimeout(function () { box.style.display = "none"; }, 380);
            }, 6000);
        });

        /* Prevent accidental double submits. Skip when another handler (e.g. the
           password guard) already cancelled the submit, so the button stays usable. */
        document.querySelectorAll("form").forEach(function (form) {
            form.addEventListener("submit", function (e) {
                if (e.defaultPrevented) { return; }
                var btn = form.querySelector("button[type=submit]");
                if (!btn || btn.disabled) { return; }
                window.setTimeout(function () { btn.disabled = true; }, 0);
            });
        });

        /* Numeric-only phone fields. */
        document.querySelectorAll("input[type=tel][inputmode=numeric]").forEach(function (input) {
            input.addEventListener("input", function () {
                var clean = input.value.replace(/[^0-9]/g, "").slice(0, 11);
                if (clean !== input.value) { input.value = clean; }
            });
        });
    }

    /* --------------------------------------------------- products flavour filter */
    function initProductFilter() {
        var bar = document.querySelector("[data-mg-filter-bar]");
        if (!bar) { return; }
        var cards = document.querySelectorAll("[data-mg-product]");
        bar.addEventListener("click", function (e) {
            var btn = e.target.closest("[data-mg-filter]");
            if (!btn) { return; }
            var value = btn.getAttribute("data-mg-filter");
            bar.querySelectorAll("[data-mg-filter]").forEach(function (b) {
                b.classList.toggle("is-active", b === btn);
                b.setAttribute("aria-pressed", b === btn ? "true" : "false");
            });
            cards.forEach(function (card) {
                var show = !value || card.getAttribute("data-mg-flavor") === value;
                card.style.display = show ? "" : "none";
            });
        });
    }

    /* ------------------------------------------------------- smooth anchor nav */
    function initAnchors() {
        document.addEventListener("click", function (e) {
            var a = e.target.closest('a[href^="#"]');
            if (!a) { return; }
            var id = a.getAttribute("href");
            if (!id || id === "#") { return; }
            var target = document.querySelector(id);
            if (!target) { return; }
            e.preventDefault();
            var top = target.getBoundingClientRect().top + window.pageYOffset - 90;
            window.scrollTo({ top: top, behavior: reduceMotion ? "auto" : "smooth" });
            history.replaceState(null, "", id);
        });
    }

    ready(function () {
        initChrome();
        initMarquees();
        initReveal();
        initMagnetic();
        initTilt();
        initCounters();
        initPasswords();
        initForms();
        initProductFilter();
        initAnchors();
    });
})();
