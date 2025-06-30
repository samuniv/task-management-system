// Theme Manager for High Contrast Support
window.themeManager = {
    currentTheme: 'default',
    storageKey: 'taskmanager-theme',

    // Initialize theme system
    initialize: function() {
        // Load saved theme from localStorage
        const savedTheme = localStorage.getItem(this.storageKey);
        const preferredTheme = savedTheme || this.detectSystemPreference();
        
        this.setTheme(preferredTheme);
        
        // Listen for system theme changes
        this.setupSystemThemeListener();
        
        // Add keyboard shortcut for theme toggle
        this.setupKeyboardShortcuts();
        
        console.log('Theme manager initialized with theme:', this.currentTheme);
    },

    // Detect system high contrast preference
    detectSystemPreference: function() {
        // Check for system high contrast preference
        if (window.matchMedia && window.matchMedia('(prefers-contrast: high)').matches) {
            return 'high-contrast';
        }
        
        // Check for reduced motion (often correlated with accessibility needs)
        if (window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
            // Don't auto-enable high contrast, but note the preference
            console.log('Reduced motion preference detected');
        }
        
        return 'default';
    },

    // Set theme and apply to document
    setTheme: function(theme) {
        if (!theme || (theme !== 'default' && theme !== 'high-contrast')) {
            theme = 'default';
        }

        // Remove previous theme
        document.documentElement.removeAttribute('data-theme');
        document.body.classList.remove('theme-default', 'theme-high-contrast');
        
        // Remove existing theme stylesheets
        const existingThemeLinks = document.querySelectorAll('link[data-theme-css]');
        existingThemeLinks.forEach(link => link.remove());

        if (theme === 'high-contrast') {
            // Set high contrast theme
            document.documentElement.setAttribute('data-theme', 'high-contrast');
            document.body.classList.add('theme-high-contrast');
            
            // Load high contrast CSS if not already loaded
            this.loadThemeCSS('css/high-contrast.css');
            
            // Announce theme change to screen readers
            this.announceThemeChange('High contrast theme activated');
            
            // Update chart colors if charts are present
            this.updateChartColors('high-contrast');
        } else {
            // Default theme
            document.documentElement.setAttribute('data-theme', 'default');
            document.body.classList.add('theme-default');
            
            this.announceThemeChange('Default theme activated');
            this.updateChartColors('default');
        }

        // Save theme preference
        this.currentTheme = theme;
        localStorage.setItem(this.storageKey, theme);
        
        // Update theme toggle button state
        this.updateThemeToggleButton();
        
        console.log('Theme set to:', theme);
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

    // Toggle between default and high contrast
    toggleHighContrast: function() {
        const newTheme = this.currentTheme === 'high-contrast' ? 'default' : 'high-contrast';
        this.setTheme(newTheme);
        return newTheme;
    },

    // Setup system theme change listener
    setupSystemThemeListener: function() {
        if (window.matchMedia) {
            const contrastQuery = window.matchMedia('(prefers-contrast: high)');
            contrastQuery.addEventListener('change', (e) => {
                if (e.matches && this.currentTheme !== 'high-contrast') {
                    console.log('System high contrast preference detected');
                    // Optionally auto-switch (commented out to avoid unwanted changes)
                    // this.setTheme('high-contrast');
                }
            });
        }
    },

    // Setup keyboard shortcuts
    setupKeyboardShortcuts: function() {
        document.addEventListener('keydown', (e) => {
            // Ctrl + Alt + H: Toggle high contrast
            if (e.ctrlKey && e.altKey && e.key === 'h') {
                e.preventDefault();
                this.toggleHighContrast();
            }
        });
    },

    // Update theme toggle button appearance
    updateThemeToggleButton: function() {
        const toggleButtons = document.querySelectorAll('.theme-toggle-button');
        toggleButtons.forEach(button => {
            const isHighContrast = this.currentTheme === 'high-contrast';
            
            // Update button text
            const iconElement = button.querySelector('i, .icon');
            const textElement = button.querySelector('.button-text');
            
            if (textElement) {
                textElement.textContent = isHighContrast ? 'Default Theme' : 'High Contrast';
            }
            
            // Update ARIA attributes
            button.setAttribute('aria-pressed', isHighContrast.toString());
            button.setAttribute('aria-label', 
                isHighContrast ? 'Switch to default theme' : 'Switch to high contrast theme');
            
            // Update visual state
            button.classList.toggle('active', isHighContrast);
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
        if (theme === 'high-contrast') {
            return {
                background: '#000000',
                text: '#ffffff',
                grid: '#ffffff',
                datasets: {
                    primary: ['#ffffff', '#ffff00', '#00ff00', '#ff0000'],
                    secondary: ['#e0e0e0', '#ffff80', '#80ff80', '#ff8080']
                }
            };
        } else {
            return {
                background: '#ffffff',
                text: '#333333',
                grid: '#e0e0e0',
                datasets: {
                    primary: ['#3b82f6', '#10b981', '#f59e0b', '#ef4444'],
                    secondary: ['#93c5fd', '#6ee7b7', '#fbbf24', '#fca5a5']
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

    // Check if user prefers high contrast
    userPrefersHighContrast: function() {
        return this.currentTheme === 'high-contrast';
    },

    // Reset to system preference
    resetToSystemPreference: function() {
        const systemTheme = this.detectSystemPreference();
        this.setTheme(systemTheme);
    },

    // Get available themes
    getAvailableThemes: function() {
        return [
            { value: 'default', label: 'Default Theme', description: 'Standard appearance' },
            { value: 'high-contrast', label: 'High Contrast', description: 'High contrast for better visibility' }
        ];
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
