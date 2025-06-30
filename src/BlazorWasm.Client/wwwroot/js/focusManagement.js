// Focus Management JavaScript for enhanced keyboard navigation and accessibility
window.focusManagement = {
    previousFocus: null,
    currentTrap: null,
    liveRegion: null,

    // Initialize focus management system
    initialize: function() {
        // Create live region for announcements if it doesn't exist
        if (!this.liveRegion) {
            this.liveRegion = document.createElement('div');
            this.liveRegion.id = 'live-announcements';
            this.liveRegion.setAttribute('aria-live', 'polite');
            this.liveRegion.setAttribute('aria-atomic', 'true');
            this.liveRegion.className = 'visually-hidden';
            document.body.appendChild(this.liveRegion);
        }

        // Add global keyboard event listeners
        this.addGlobalKeyboardListeners();
    },

    // Add global keyboard event listeners for accessibility shortcuts
    addGlobalKeyboardListeners: function() {
        document.addEventListener('keydown', (e) => {
            // Alt + 1: Skip to main content
            if (e.altKey && e.key === '1') {
                e.preventDefault();
                this.moveFocusToMainContent();
            }
            // Alt + 2: Skip to navigation
            else if (e.altKey && e.key === '2') {
                e.preventDefault();
                this.moveFocusToNavigation();
            }
            // Escape: Close modals and release focus traps
            else if (e.key === 'Escape') {
                if (this.currentTrap) {
                    this.releaseFocusTrap();
                }
            }
        });
    },

    // Focus a specific element by ID
    focusElement: function(elementId) {
        try {
            const element = document.getElementById(elementId);
            if (element) {
                // Make element focusable if it's not already
                if (!element.hasAttribute('tabindex') && !this.isFocusableElement(element)) {
                    element.setAttribute('tabindex', '-1');
                }
                element.focus();
                
                // Scroll element into view if needed
                element.scrollIntoView({ 
                    behavior: 'smooth', 
                    block: 'center' 
                });
                
                return true;
            }
            console.warn(`Element with ID '${elementId}' not found`);
            return false;
        } catch (error) {
            console.error('Error focusing element:', error);
            return false;
        }
    },

    // Focus the first focusable element in a container
    focusFirstElement: function(containerId) {
        try {
            const container = document.getElementById(containerId) || document.querySelector(containerId);
            if (!container) {
                console.warn(`Container '${containerId}' not found`);
                return false;
            }

            const focusableElement = this.getFocusableElements(container)[0];
            if (focusableElement) {
                focusableElement.focus();
                return true;
            }
            return false;
        } catch (error) {
            console.error('Error focusing first element:', error);
            return false;
        }
    },

    // Get all focusable elements within a container
    getFocusableElements: function(container) {
        const focusableSelectors = [
            'button:not([disabled])',
            'input:not([disabled])',
            'select:not([disabled])',
            'textarea:not([disabled])',
            'a[href]',
            'area[href]',
            '[tabindex]:not([tabindex="-1"])',
            '[contenteditable="true"]',
            'audio[controls]',
            'video[controls]',
            'iframe',
            'object',
            'embed',
            'details summary'
        ].join(', ');

        return Array.from(container.querySelectorAll(focusableSelectors))
            .filter(element => {
                return this.isVisible(element) && !element.hasAttribute('disabled');
            });
    },

    // Check if element is focusable
    isFocusableElement: function(element) {
        const focusableTags = ['A', 'BUTTON', 'INPUT', 'SELECT', 'TEXTAREA'];
        return focusableTags.includes(element.tagName) || 
               element.hasAttribute('tabindex') ||
               element.hasAttribute('contenteditable');
    },

    // Check if element is visible
    isVisible: function(element) {
        const style = window.getComputedStyle(element);
        return style.display !== 'none' && 
               style.visibility !== 'hidden' && 
               element.offsetParent !== null;
    },

    // Trap focus within a container (for modals, dialogs, etc.)
    trapFocus: function(containerId) {
        try {
            const container = document.getElementById(containerId) || document.querySelector(containerId);
            if (!container) {
                console.warn(`Container '${containerId}' not found for focus trap`);
                return false;
            }

            // Save current focus to restore later
            this.saveCurrentFocus();

            // Release any existing trap
            this.releaseFocusTrap();

            // Set up new trap
            this.currentTrap = {
                container: container,
                focusableElements: this.getFocusableElements(container),
                keydownHandler: (e) => this.handleTrapKeydown(e)
            };

            // Add event listener
            document.addEventListener('keydown', this.currentTrap.keydownHandler);

            // Focus first element
            if (this.currentTrap.focusableElements.length > 0) {
                this.currentTrap.focusableElements[0].focus();
            }

            // Add trap indicator to container
            container.setAttribute('data-focus-trap', 'true');

            return true;
        } catch (error) {
            console.error('Error setting up focus trap:', error);
            return false;
        }
    },

    // Handle keydown events within focus trap
    handleTrapKeydown: function(e) {
        if (!this.currentTrap || e.key !== 'Tab') return;

        const { focusableElements } = this.currentTrap;
        if (focusableElements.length === 0) return;

        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];
        const activeElement = document.activeElement;

        if (e.shiftKey) {
            // Shift + Tab: moving backwards
            if (activeElement === firstElement) {
                e.preventDefault();
                lastElement.focus();
            }
        } else {
            // Tab: moving forwards
            if (activeElement === lastElement) {
                e.preventDefault();
                firstElement.focus();
            }
        }
    },

    // Release focus trap
    releaseFocusTrap: function() {
        try {
            if (this.currentTrap) {
                // Remove event listener
                document.removeEventListener('keydown', this.currentTrap.keydownHandler);

                // Remove trap indicator
                this.currentTrap.container.removeAttribute('data-focus-trap');

                // Clear trap reference
                this.currentTrap = null;

                // Restore previous focus
                this.restorePreviousFocus();

                return true;
            }
            return false;
        } catch (error) {
            console.error('Error releasing focus trap:', error);
            return false;
        }
    },

    // Set focus within a container without trapping
    setFocusWithin: function(containerId) {
        try {
            const container = document.getElementById(containerId) || document.querySelector(containerId);
            if (container) {
                const focusableElements = this.getFocusableElements(container);
                if (focusableElements.length > 0) {
                    focusableElements[0].focus();
                    return true;
                }
            }
            return false;
        } catch (error) {
            console.error('Error setting focus within container:', error);
            return false;
        }
    },

    // Save current focus element
    saveCurrentFocus: function() {
        this.previousFocus = document.activeElement;
    },

    // Restore previously focused element
    restorePreviousFocus: function() {
        try {
            if (this.previousFocus && this.isVisible(this.previousFocus)) {
                this.previousFocus.focus();
                this.previousFocus = null;
                return true;
            }
            return false;
        } catch (error) {
            console.error('Error restoring focus:', error);
            return false;
        }
    },

    // Announce text to screen readers
    announceLiveText: function(text, priority = 'polite') {
        try {
            if (!this.liveRegion) {
                this.initialize();
            }

            // Set priority level
            this.liveRegion.setAttribute('aria-live', priority);

            // Clear and set new text
            this.liveRegion.textContent = '';
            setTimeout(() => {
                this.liveRegion.textContent = text;
            }, 100);

            return true;
        } catch (error) {
            console.error('Error announcing live text:', error);
            return false;
        }
    },

    // Move focus to top of page
    moveFocusToTop: function() {
        try {
            // Focus skip link or first focusable element
            const skipLink = document.querySelector('.skip-link');
            const firstFocusable = this.getFocusableElements(document.body)[0];
            
            if (skipLink) {
                skipLink.focus();
            } else if (firstFocusable) {
                firstFocusable.focus();
            } else {
                // Focus body as fallback
                document.body.focus();
            }
            
            window.scrollTo({ top: 0, behavior: 'smooth' });
            return true;
        } catch (error) {
            console.error('Error moving focus to top:', error);
            return false;
        }
    },

    // Move focus to main content
    moveFocusToMainContent: function() {
        try {
            const mainContent = document.querySelector('main') || 
                             document.querySelector('[role="main"]') ||
                             document.querySelector('.main-content') ||
                             document.getElementById('main-content');

            if (mainContent) {
                // Make main content focusable if it's not already
                if (!mainContent.hasAttribute('tabindex')) {
                    mainContent.setAttribute('tabindex', '-1');
                }
                mainContent.focus();
                
                // Scroll to main content
                mainContent.scrollIntoView({ 
                    behavior: 'smooth', 
                    block: 'start' 
                });
                
                this.announceLiveText('Jumped to main content');
                return true;
            }
            return false;
        } catch (error) {
            console.error('Error moving focus to main content:', error);
            return false;
        }
    },

    // Move focus to navigation
    moveFocusToNavigation: function() {
        try {
            const navigation = document.querySelector('nav') ||
                             document.querySelector('[role="navigation"]') ||
                             document.querySelector('.navigation') ||
                             document.getElementById('navigation');

            if (navigation) {
                const firstFocusable = this.getFocusableElements(navigation)[0];
                if (firstFocusable) {
                    firstFocusable.focus();
                    this.announceLiveText('Jumped to navigation');
                    return true;
                }
            }
            return false;
        } catch (error) {
            console.error('Error moving focus to navigation:', error);
            return false;
        }
    },

    // Enhanced keyboard navigation for custom components
    enhanceKeyboardNavigation: function() {
        // Add arrow key navigation for button groups
        this.addArrowKeyNavigation();
        
        // Add Enter/Space support for custom clickable elements
        this.addCustomClickSupport();
        
        // Add Escape key support for closing dialogs/dropdowns
        this.addEscapeKeySupport();
    },

    // Add arrow key navigation for button groups and menus
    addArrowKeyNavigation: function() {
        document.addEventListener('keydown', (e) => {
            const target = e.target;
            const parent = target.closest('[role="group"], [role="toolbar"], [role="menubar"]');
            
            if (!parent || !['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown'].includes(e.key)) {
                return;
            }

            e.preventDefault();
            
            const focusableElements = this.getFocusableElements(parent);
            const currentIndex = focusableElements.indexOf(target);
            
            if (currentIndex === -1) return;

            let nextIndex;
            const isHorizontal = parent.getAttribute('aria-orientation') !== 'vertical';

            if ((isHorizontal && e.key === 'ArrowRight') || (!isHorizontal && e.key === 'ArrowDown')) {
                nextIndex = (currentIndex + 1) % focusableElements.length;
            } else if ((isHorizontal && e.key === 'ArrowLeft') || (!isHorizontal && e.key === 'ArrowUp')) {
                nextIndex = (currentIndex - 1 + focusableElements.length) % focusableElements.length;
            } else {
                return;
            }

            focusableElements[nextIndex].focus();
        });
    },

    // Add Enter/Space support for custom clickable elements
    addCustomClickSupport: function() {
        document.addEventListener('keydown', (e) => {
            if (e.key !== 'Enter' && e.key !== ' ') return;

            const target = e.target;
            
            // Check if element has click handler or is marked as clickable
            if (target.hasAttribute('role') && 
                ['button', 'link', 'menuitem', 'option', 'tab'].includes(target.getAttribute('role'))) {
                
                // Prevent default for space key to avoid page scroll
                if (e.key === ' ') {
                    e.preventDefault();
                }
                
                // Trigger click event
                target.click();
            }
        });
    },

    // Add Escape key support for closing dialogs and dropdowns
    addEscapeKeySupport: function() {
        document.addEventListener('keydown', (e) => {
            if (e.key !== 'Escape') return;

            // Close open dropdowns
            const openDropdowns = document.querySelectorAll('[aria-expanded="true"]');
            openDropdowns.forEach(dropdown => {
                dropdown.setAttribute('aria-expanded', 'false');
                dropdown.focus();
            });

            // Close modals
            const openModals = document.querySelectorAll('[role="dialog"][aria-hidden="false"]');
            openModals.forEach(modal => {
                const closeButton = modal.querySelector('[data-dismiss="modal"], .modal-close');
                if (closeButton) {
                    closeButton.click();
                }
            });
        });
    }
};

// Initialize focus management when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.focusManagement.initialize();
        window.focusManagement.enhanceKeyboardNavigation();
    });
} else {
    window.focusManagement.initialize();
    window.focusManagement.enhanceKeyboardNavigation();
}
