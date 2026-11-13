// Keeps the layout and labels usable when an external menu photo cannot load.
document.addEventListener("error", (event) => {
    const image = event.target;
    if (image instanceof HTMLImageElement) image.classList.add("photo-unavailable");
}, true);
