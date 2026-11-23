// Progressive enhancement only — the game is fully playable without JavaScript.

// Right-click a hidden cell to flag it.
document.getElementById('board')?.addEventListener('contextmenu', (e) => {
    const cell = e.target.closest('button.cell.hidden');
    if (!cell) return;
    e.preventDefault();
    const form = document.getElementById('flag-form');
    form.elements.row.value = cell.dataset.row;
    form.elements.col.value = cell.dataset.col;
    form.submit();
});

// Keep the timer ticking between requests.
const timer = document.getElementById('timer');
if (timer && timer.dataset.running === 'true' && timer.dataset.started) {
    const started = Number(timer.dataset.started);
    const tick = () => {
        const seconds = Math.min(999, Math.floor((Date.now() - started) / 1000));
        timer.textContent = String(seconds).padStart(3, '0');
    };
    tick();
    setInterval(tick, 1000);
}
