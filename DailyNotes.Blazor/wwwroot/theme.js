function setTheme(theme) {
    const html = document.documentElement;
    localStorage.setItem('theme-preference', theme);
    
    if (theme === 'device') {
        html.removeAttribute('data-theme');
    } else {
        html.setAttribute('data-theme', theme);
    }
}

function getPreferredTheme() {
    return localStorage.getItem('theme-preference') || 'device';
}

// Initialize
(function() {
    setTheme(getPreferredTheme());
})();

window.themeManager = {
    setTheme: setTheme,
    getTheme: getPreferredTheme
};
