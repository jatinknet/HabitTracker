window.chartInstances = window.chartInstances || {};

function destroyIfExists(id) {
    if (window.chartInstances[id]) {
        window.chartInstances[id].destroy();
        delete window.chartInstances[id];
    }
}

// Donut chart used for week progress rings and the overall monthly progress ring
window.renderDonut = function (canvasId, percent, color) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    destroyIfExists(canvasId);

    window.chartInstances[canvasId] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            datasets: [{
                data: [percent, Math.max(0, 100 - percent)],
                backgroundColor: [color, '#eef0f6'],
                borderWidth: 0
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '72%',
            plugins: { legend: { display: false }, tooltip: { enabled: false } }
        }
    });
};

// Horizontal bar chart for per-habit completion breakdown
window.renderHabitBreakdownChart = function (canvasId, labels, values, colors) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    destroyIfExists(canvasId);

    window.chartInstances[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: colors,
                borderRadius: 8,
                maxBarThickness: 22
            }]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: { max: 100, grid: { display: false }, ticks: { callback: v => v + '%' } },
                y: { grid: { display: false } }
            },
            plugins: { legend: { display: false } }
        }
    });
};

// Line chart for 6-month completion trend
window.renderTrendChart = function (canvasId, labels, values) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    destroyIfExists(canvasId);

    window.chartInstances[canvasId] = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                borderColor: '#2e86de',
                backgroundColor: 'rgba(46,134,222,0.12)',
                fill: true,
                tension: 0.35,
                pointBackgroundColor: '#2e86de',
                pointRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: { min: 0, max: 100, ticks: { callback: v => v + '%' } },
                x: { grid: { display: false } }
            },
            plugins: { legend: { display: false } }
        }
    });
};
