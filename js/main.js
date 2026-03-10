// Mobile nav toggle
const navToggle = document.getElementById("navToggle");
const navMenu = document.getElementById("navMenu");

if (navToggle && navMenu) {
  navToggle.addEventListener("click", () => {
    const open = navMenu.classList.toggle("is-open");
    navToggle.setAttribute("aria-expanded", String(open));
  });

  // Close menu on link click (mobile)
  navMenu.querySelectorAll("a").forEach(a => {
    a.addEventListener("click", () => {
      navMenu.classList.remove("is-open");
      navToggle.setAttribute("aria-expanded", "false");
    });
  });
}

// Booking form success state (prototype)
const form = document.getElementById("bookingForm");
const success = document.getElementById("formSuccess");
if (form && success) {
  form.addEventListener("submit", (e) => {
    e.preventDefault();
    success.hidden = false;
    form.reset();
    success.scrollIntoView({ behavior: "smooth", block: "center" });
  });
}

// Lightbox for preview grid
const lightbox = document.getElementById("lightbox");
const lightboxImg = document.getElementById("lightboxImg");
const lightboxClose = document.getElementById("lightboxClose");

function openLightbox(src) {
  if (!lightbox || !lightboxImg) return;
  lightboxImg.src = src;
  lightbox.hidden = false;
  document.body.style.overflow = "hidden";
}

function closeLightbox() {
  if (!lightbox) return;
  lightbox.hidden = true;
  document.body.style.overflow = "";
  if (lightboxImg) lightboxImg.src = "";
}

document.querySelectorAll(".thumb[data-full]").forEach(btn => {
  btn.addEventListener("click", () => openLightbox(btn.dataset.full));
});

if (lightboxClose) lightboxClose.addEventListener("click", closeLightbox);
if (lightbox) {
  lightbox.addEventListener("click", (e) => {
    if (e.target === lightbox) closeLightbox();
  });
  window.addEventListener("keydown", (e) => {
    if (e.key === "Escape" && !lightbox.hidden) closeLightbox();
  });
}

// Packages reveal interaction
const packagesToggle = document.getElementById("packagesToggle");
const packagesContent = document.getElementById("packagesContent");
const openPackages = document.getElementById("openPackages");

function setPackagesOpen(open) {
  if (!packagesToggle || !packagesContent) return;
  packagesContent.hidden = !open;
  packagesToggle.setAttribute("aria-expanded", String(open));
  packagesToggle.textContent = open ? "Hide Packages" : "Packages";
}

if (packagesToggle && packagesContent) {
  packagesToggle.addEventListener("click", () => {
    setPackagesOpen(packagesContent.hidden);
  });
}

if (openPackages && packagesContent) {
  openPackages.addEventListener("click", () => {
    setPackagesOpen(true);
  });
}
