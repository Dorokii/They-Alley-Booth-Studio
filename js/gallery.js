const grid = document.getElementById("galleryGrid");
const chips = document.querySelectorAll(".chip");
const lightbox = document.getElementById("lightbox");
const lightboxImg = document.getElementById("lightboxImg");
const lightboxClose = document.getElementById("lightboxClose");

function setActiveChip(btn) {
  chips.forEach(c => c.classList.remove("is-active"));
  btn.classList.add("is-active");
}

function applyFilter(filter) {
  if (!grid) return;
  grid.querySelectorAll(".thumb").forEach(item => {
    const cat = item.dataset.cat;
    const show = filter === "all" || cat === filter;
    item.style.display = show ? "" : "none";
  });
}

chips.forEach(btn => {
  btn.addEventListener("click", () => {
    setActiveChip(btn);
    applyFilter(btn.dataset.filter);
  });
});

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
