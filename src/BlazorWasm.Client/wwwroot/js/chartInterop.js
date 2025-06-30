// Chart.js interop functions for Blazor
window.chartInterop = {
  charts: new Map(),

  // Initialize or update a chart
  initializeChart: function (canvasId, chartType, data, options) {
    try {
      const canvas = document.getElementById(canvasId);
      if (!canvas) {
        console.error(`Canvas element with id '${canvasId}' not found`);
        return false;
      }

      // Destroy existing chart if it exists
      if (this.charts.has(canvasId)) {
        this.charts.get(canvasId).destroy();
      }

      const ctx = canvas.getContext("2d");

      // Enhanced accessibility options
      const accessibilityOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          ...options.plugins,
          // Ensure tooltips are accessible
          tooltip: {
            ...options.plugins?.tooltip,
            enabled: true,
            displayColors: true,
            titleAlign: "center",
            bodyAlign: "center",
          },
        },
        // Add keyboard navigation support
        onHover: (event, activeElements) => {
          canvas.style.cursor =
            activeElements.length > 0 ? "pointer" : "default";
        },
        // Enhance focus management
        onFocus: () => {
          canvas.setAttribute("aria-expanded", "true");
        },
        onBlur: () => {
          canvas.setAttribute("aria-expanded", "false");
        },
      };

      const chart = new Chart(ctx, {
        type: chartType,
        data: data,
        options: {
          ...accessibilityOptions,
          ...options,
        },
      });

      // Set accessibility attributes on canvas
      canvas.setAttribute("tabindex", "0");
      canvas.setAttribute("role", "img");

      // Add keyboard event listeners for accessibility
      canvas.addEventListener("keydown", (e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          // Trigger chart interaction
          const rect = canvas.getBoundingClientRect();
          const syntheticEvent = {
            type: "click",
            clientX: rect.left + rect.width / 2,
            clientY: rect.top + rect.height / 2,
          };
          chart.canvas.dispatchEvent(new MouseEvent("click", syntheticEvent));
        }
      });

      this.charts.set(canvasId, chart);
      return true;
    } catch (error) {
      console.error("Error initializing chart:", error);
      return false;
    }
  },

  // Update chart data
  updateChart: function (canvasId, data) {
    try {
      const chart = this.charts.get(canvasId);
      if (!chart) {
        console.error(`Chart with id '${canvasId}' not found`);
        return false;
      }

      chart.data = data;
      chart.update();
      return true;
    } catch (error) {
      console.error("Error updating chart:", error);
      return false;
    }
  },

  // Destroy a chart
  destroyChart: function (canvasId) {
    try {
      const chart = this.charts.get(canvasId);
      if (chart) {
        chart.destroy();
        this.charts.delete(canvasId);
        return true;
      }
      return false;
    } catch (error) {
      console.error("Error destroying chart:", error);
      return false;
    }
  },

  // Create a doughnut chart for task status
  createStatusChart: function (canvasId, statusData) {
    const data = {
      labels: ["Pending", "In Progress", "Done", "Cancelled"],
      datasets: [
        {
          data: [
            statusData.pending,
            statusData.inProgress,
            statusData.done,
            statusData.cancelled,
          ],
          backgroundColor: [
            "#fbbf24", // yellow for pending
            "#3b82f6", // blue for in progress
            "#10b981", // green for done
            "#ef4444", // red for cancelled
          ],
          borderWidth: 2,
          borderColor: "#ffffff",
        },
      ],
    };

    const options = {
      plugins: {
        legend: {
          position: "bottom",
          labels: {
            padding: 20,
            usePointStyle: true,
            generateLabels: function (chart) {
              const data = chart.data;
              if (data.labels.length && data.datasets.length) {
                return data.labels.map((label, i) => {
                  const value = data.datasets[0].data[i];
                  const total = data.datasets[0].data.reduce(
                    (a, b) => a + b,
                    0
                  );
                  const percentage =
                    total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                  return {
                    text: `${label}: ${value} (${percentage}%)`,
                    fillStyle: data.datasets[0].backgroundColor[i],
                    strokeStyle: data.datasets[0].borderColor,
                    lineWidth: data.datasets[0].borderWidth,
                    pointStyle: "circle",
                    hidden: false,
                    index: i,
                  };
                });
              }
              return [];
            },
          },
        },
        tooltip: {
          callbacks: {
            label: function (context) {
              const label = context.label || "";
              const value = context.parsed;
              const total = context.dataset.data.reduce((a, b) => a + b, 0);
              const percentage =
                total > 0 ? ((value / total) * 100).toFixed(1) : 0;
              return `${label}: ${value} tasks (${percentage}%)`;
            },
          },
        },
      },
      // Add accessibility description
      accessibility: {
        description: `Task status distribution showing ${statusData.pending} pending, ${statusData.inProgress} in progress, ${statusData.done} completed, and ${statusData.cancelled} cancelled tasks`,
      },
    };

    return this.initializeChart(canvasId, "doughnut", data, options);
  },

  // Create a bar chart for task priority
  createPriorityChart: function (canvasId, priorityData) {
    const data = {
      labels: ["Low", "Medium", "High", "Critical"],
      datasets: [
        {
          label: "Tasks by Priority",
          data: [
            priorityData.low,
            priorityData.medium,
            priorityData.high,
            priorityData.critical,
          ],
          backgroundColor: [
            "#6b7280", // gray for low
            "#3b82f6", // blue for medium
            "#f59e0b", // amber for high
            "#ef4444", // red for critical
          ],
          borderRadius: 4,
          borderSkipped: false,
        },
      ],
    };

    const options = {
      plugins: {
        legend: {
          display: false,
        },
        tooltip: {
          callbacks: {
            title: function (context) {
              return `${context[0].label} Priority Tasks`;
            },
            label: function (context) {
              const value = context.parsed.y;
              const label = context.label;
              return `${label}: ${value} task${value !== 1 ? "s" : ""}`;
            },
          },
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            stepSize: 1,
          },
          title: {
            display: true,
            text: "Number of Tasks",
          },
        },
        x: {
          title: {
            display: true,
            text: "Priority Level",
          },
        },
      },
      // Add accessibility description
      accessibility: {
        description: `Task priority distribution showing ${priorityData.low} low priority, ${priorityData.medium} medium priority, ${priorityData.high} high priority, and ${priorityData.critical} critical priority tasks`,
      },
    };

    return this.initializeChart(canvasId, "bar", data, options);
  },

  // Create a line chart for completion trend
  createTrendChart: function (canvasId, trendData) {
    const labels = trendData.map((item) => {
      const date = new Date(item.date);
      return date.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
      });
    });

    const data = {
      labels: labels,
      datasets: [
        {
          label: "Completed Tasks",
          data: trendData.map((item) => item.completedTasks),
          borderColor: "#10b981",
          backgroundColor: "rgba(16, 185, 129, 0.1)",
          fill: true,
          tension: 0.4,
          pointBackgroundColor: "#10b981",
          pointBorderColor: "#ffffff",
          pointBorderWidth: 2,
          pointRadius: 4,
        },
        {
          label: "Created Tasks",
          data: trendData.map((item) => item.createdTasks),
          borderColor: "#3b82f6",
          backgroundColor: "rgba(59, 130, 246, 0.1)",
          fill: true,
          tension: 0.4,
          pointBackgroundColor: "#3b82f6",
          pointBorderColor: "#ffffff",
          pointBorderWidth: 2,
          pointRadius: 4,
        },
      ],
    };

    const options = {
      plugins: {
        legend: {
          position: "top",
          labels: {
            usePointStyle: true,
            padding: 20,
          },
        },
        tooltip: {
          mode: "index",
          intersect: false,
          callbacks: {
            title: function (context) {
              return `Date: ${context[0].label}`;
            },
            label: function (context) {
              const value = context.parsed.y;
              const dataset = context.dataset.label;
              return `${dataset}: ${value} task${value !== 1 ? "s" : ""}`;
            },
          },
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            stepSize: 1,
          },
          title: {
            display: true,
            text: "Number of Tasks",
          },
        },
        x: {
          ticks: {
            maxTicksLimit: 10,
          },
          title: {
            display: true,
            text: "Date",
          },
        },
      },
      interaction: {
        intersect: false,
        mode: "index",
      },
      // Add accessibility description
      accessibility: {
        description: `Task completion trend showing daily task creation and completion over the last ${trendData.length} days`,
      },
    };

    return this.initializeChart(canvasId, "line", data, options);
  },
};
