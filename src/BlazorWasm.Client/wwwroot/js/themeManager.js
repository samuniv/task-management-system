// Enhanced Theme Manager for Dark/Light Mode and High Contrast Support
window.themeManager = {
    currentTheme: 'auto',
    storageKey: 'taskmanager-theme',
    
    // Available themes
    themes: {
        'auto': { 
            label: 'Auto (System)', 
            description: 'Follow system preference',
            icon: 'fa-circle-half-stroke'
        },
        'light': { 
            label: 'Light', 
            description: 'Light theme',
            icon: 'fa-sun'
        },
        'dark': { 
            label: 'Dark', 
            description: 'Dark theme',
            icon: 'fa-moon'
        },
        'high-contrast': { 
            label: 'High Contrast', 
            description: 'High contrast for accessibility',
            icon: 'fa-adjust'
        }
    },

    // Initialize theme system
    initialize: function() {
        console.log('Initializing theme manager...');
        
        // Load saved theme from localStorage
        const savedTheme = localStorage.getItem(this.storageKey);
        const preferredTheme = savedTheme || 'auto';
        
        this.setTheme(preferredTheme);
        
        // Listen for system theme changes
        this.setupSystemThemeListener();
        
        // Add keyboard shortcuts for theme toggling
        this.setupKeyboardShortcuts();
        
        console.log('Theme manager initialized with theme:', this.currentTheme);
    },

    // Detect system color scheme preference
    detectSystemPreference: function() {
        if (!window.matchMedia) {
            return 'light';
        }
        
        // Check for high contrast preference first
        if (window.matchMedia('(prefers-contrast: high)').matches) {
            return 'high-contrast';
        }
        
        // Check for dark color scheme
        if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 'dark';
        }
        
        // Default to light
        return 'light';
    },

    // Get effective theme (resolves 'auto' to actual theme)
    getEffectiveTheme: function(theme = this.currentTheme) {
        if (theme === 'auto') {
            return this.detectSystemPreference();
        }
        return theme;
    },

    // Set theme and apply to document
    setTheme: function(theme) {
        // Validate theme
        if (!theme || !this.themes[theme]) {
            theme = 'auto';
        }

        const effectiveTheme = this.getEffectiveTheme(theme);
        
        // Remove previous theme classes and attributes
        document.documentElement.removeAttribute('data-theme');
        document.body.classList.remove('theme-auto', 'theme-light', 'theme-dark', 'theme-high-contrast');
        
        // Remove existing theme stylesheets
        const existingThemeLinks = document.querySelectorAll('link[data-theme-css]');
        existingThemeLinks.forEach(link => link.remove());

        // Apply new theme
        document.documentElement.setAttribute('data-theme', effectiveTheme);
        document.body.classList.add(`theme-${effectiveTheme}`);
        
        // Load additional CSS for specific themes
        if (effectiveTheme === 'dark') {
            this.loadThemeCSS('css/dark-theme.css');
        } else if (effectiveTheme === 'high-contrast') {
            this.loadThemeCSS('css/high-contrast.css');
        }

        // Save theme preference (save the selected theme, not the effective theme)
        this.currentTheme = theme;
        localStorage.setItem(this.storageKey, theme);
        
        // Announce theme change to screen readers
        this.announceThemeChange(`${this.themes[theme].label} theme activated`);
        
        // Update chart colors if charts are present
        this.updateChartColors(effectiveTheme);
        
        // Update theme toggle button state
        this.updateThemeToggleButton();
        
        console.log(`Theme set to: ${theme} (effective: ${effectiveTheme})`);
    },

    // Load theme CSS file
    loadThemeCSS: function(href) {
        // Check if already loaded
        if (document.querySelector(`link[href="${href}"]`)) {
            return;
        }

        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = href;
        link.setAttribute('data-theme-css', 'true');
        document.head.appendChild(link);
    },

    // Get current theme
    getCurrentTheme: function() {
        return this.currentTheme;
    },

    // Toggle between themes
    toggleTheme: function() {
        const currentEffective = this.getEffectiveTheme();
        let newTheme;
        
        // Cycle through themes: auto -> light -> dark -> high-contrast -> auto
        switch (this.currentTheme) {
            case 'auto':
                newTheme = 'light';
                break;
            case 'light':
                newTheme = 'dark';
                break;
            case 'dark':
                newTheme = 'high-contrast';
                break;
            case 'high-contrast':
                newTheme = 'auto';
                break;
            default:
                newTheme = 'auto';
        }
        
        this.setTheme(newTheme);
        return newTheme;
    },

    // Toggle high contrast (legacy method for backwards compatibility)
    toggleHighContrast: function() {
        const currentEffective = this.getEffectiveTheme();
        const newTheme = currentEffective === 'high-contrast' ? 'auto' : 'high-contrast';
        this.setTheme(newTheme);
        return newTheme;
    },

    // Setup system theme change listener
    setupSystemThemeListener: function() {
        if (!window.matchMedia) {
            return;
        }

        // Listen for dark mode changes
        const darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)');
        darkModeQuery.addEventListener('change', (e) => {
            if (this.currentTheme === 'auto') {
                console.log('System dark mode preference changed:', e.matches);
                this.setTheme('auto'); // Re-apply auto theme to pick up system changes
            }
        });

        // Listen for high contrast changes
        const contrastQuery = window.matchMedia('(prefers-contrast: high)');
        contrastQuery.addEventListener('change', (e) => {
            if (this.currentTheme === 'auto') {
                console.log('System high contrast preference changed:', e.matches);
                this.setTheme('auto'); // Re-apply auto theme to pick up system changes
            }
        });
    },

    // Setup keyboard shortcuts
    setupKeyboardShortcuts: function() {
        document.addEventListener('keydown', (e) => {
            // Ctrl + Alt + T: Toggle theme
            if (e.ctrlKey && e.altKey && e.key === 't') {
                e.preventDefault();
                this.toggleTheme();
            }
            // Ctrl + Alt + H: Toggle high contrast (legacy)
            else if (e.ctrlKey && e.altKey && e.key === 'h') {
                e.preventDefault();
                this.toggleHighContrast();
            }
        });
    },

    // Update theme toggle button appearance
    updateThemeToggleButton: function() {
        const toggleButtons = document.querySelectorAll('.theme-toggle-button');
        const currentTheme = this.themes[this.currentTheme];
        const effectiveTheme = this.getEffectiveTheme();
        
        toggleButtons.forEach(button => {
            // Update button icon
            const iconElement = button.querySelector('i, .icon');
            if (iconElement) {
                // Remove all theme icon classes
                iconElement.className = iconElement.className.replace(/fa-[a-z-]+/g, '');
                iconElement.classList.add('fas', currentTheme.icon);
            }
            
            // Update button text
            const textElement = button.querySelector('.button-text');
            if (textElement) {
                textElement.textContent = currentTheme.label;
            }
            
            // Update ARIA attributes
            button.setAttribute('aria-pressed', (effectiveTheme !== 'light').toString());
            button.setAttribute('aria-label', `Current theme: ${currentTheme.label}. Click to change theme.`);
            button.title = `${currentTheme.description} (Ctrl+Alt+T)`;
            
            // Update visual state based on effective theme
            button.classList.toggle('active', effectiveTheme !== 'light');
        });
    },

    // Announce theme changes to screen readers
    announceThemeChange: function(message) {
        // Create or update live region
        let liveRegion = document.getElementById('theme-announcement');
        if (!liveRegion) {
            liveRegion = document.createElement('div');
            liveRegion.id = 'theme-announcement';
            liveRegion.setAttribute('aria-live', 'polite');
            liveRegion.setAttribute('aria-atomic', 'true');
            liveRegion.className = 'visually-hidden';
            document.body.appendChild(liveRegion);
        }
        
        // Clear and set new message
        liveRegion.textContent = '';
        setTimeout(() => {
            liveRegion.textContent = message;
        }, 100);
    },

    // Update chart colors when theme changes
    updateChartColors: function(theme) {
        if (typeof window.chartInterop !== 'undefined') {
            const chartColors = this.getChartColors(theme);
            
            // Update existing charts
            const charts = window.chartInterop.charts;
            if (charts) {
                charts.forEach((chart, canvasId) => {
                    this.updateChartTheme(chart, chartColors);
                });
            }
        }
    },

    // Get chart colors for theme
    getChartColors: function(theme) {
        switch (theme) {
            case 'dark':
                return {
                    background: '#1e1e1e',
                    text: '#e4e6ea',
                    grid: '#495057',
                    datasets: {
                        primary: ['#0d6efd', '#198754', '#ffc107', '#dc3545'],
                        secondary: ['#4fc3f7', '#66bb6a', '#ffeb3b', '#f06292']
                    }
                };
            case 'high-contrast':
                return {
                    background: '#000000',
                    text: '#ffffff',
                    grid: '#ffffff',
                    datasets: {
                        primary: ['#ffffff', '#ffff00', '#00ff00', '#ff0000'],
                        secondary: ['#e0e0e0', '#ffff80', '#80ff80', '#ff8080']
                    }
                };
            default: // light theme
                return {
                    background: '#ffffff',
                    text: '#212529',
                    grid: '#dee2e6',
                    datasets: {
                        primary: ['#0d6efd', '#198754', '#ffc107', '#dc3545'],
                        secondary: ['#a8c5ff', '#a3e6a3', '#fff3a0', '#f5a3a3']
                    }
                };
        }
    },

    // Update individual chart theme
    updateChartTheme: function(chart, colors) {
        try {
            // Update chart options
            if (chart.options) {
                chart.options.plugins = chart.options.plugins || {};
                chart.options.plugins.legend = chart.options.plugins.legend || {};
                chart.options.plugins.legend.labels = chart.options.plugins.legend.labels || {};
                chart.options.plugins.legend.labels.color = colors.text;
                
                if (chart.options.scales) {
                    Object.keys(chart.options.scales).forEach(scaleKey => {
                        const scale = chart.options.scales[scaleKey];
                        if (scale.ticks) {
                            scale.ticks.color = colors.text;
                        }
                        if (scale.grid) {
                            scale.grid.color = colors.grid;
                        }
                    });
                }
            }
            
            // Update dataset colors
            if (chart.data && chart.data.datasets) {
                chart.data.datasets.forEach((dataset, index) => {
                    if (dataset.backgroundColor && Array.isArray(dataset.backgroundColor)) {
                        dataset.backgroundColor = colors.datasets.primary;
                        dataset.borderColor = colors.datasets.primary;
                    } else if (typeof dataset.backgroundColor === 'string') {
                        dataset.backgroundColor = colors.datasets.primary[index % colors.datasets.primary.length];
                        dataset.borderColor = colors.datasets.primary[index % colors.datasets.primary.length];
                    }
                });
            }
            
            // Update chart
            chart.update();
        } catch (error) {
            console.error('Error updating chart theme:', error);
        }
    },

    // Check if user prefers dark mode
    userPrefersDarkMode: function() {
        const effectiveTheme = this.getEffectiveTheme();
        return effectiveTheme === 'dark';
    },

    // Check if user prefers high contrast (legacy method for backwards compatibility)
    userPrefersHighContrast: function() {
        const effectiveTheme = this.getEffectiveTheme();
        return effectiveTheme === 'high-contrast';
    },

    // Reset to system preference
    resetToSystemPreference: function() {
        this.setTheme('auto');
    },

    // Get available themes
    getAvailableThemes: function() {
        return Object.keys(this.themes).map(key => ({
            value: key,
            label: this.themes[key].label,
            description: this.themes[key].description,
            icon: this.themes[key].icon,
            isCurrent: key === this.currentTheme,
            isEffective: key === this.getEffectiveTheme()
        }));
    },

    // Get current theme info
    getCurrentThemeInfo: function() {
        return {
            selected: this.currentTheme,
            effective: this.getEffectiveTheme(),
            label: this.themes[this.currentTheme]?.label || 'Unknown',
            description: this.themes[this.currentTheme]?.description || '',
            icon: this.themes[this.currentTheme]?.icon || 'fa-circle'
        };
    },

    // Check if theme is system-managed
    isSystemManaged: function() {
        return this.currentTheme === 'auto';
    }
};

// Initialize theme manager when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.themeManager.initialize();
    });
} else {
    window.themeManager.initialize();
}
