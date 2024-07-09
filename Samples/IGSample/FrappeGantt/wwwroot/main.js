(function () {
    function create(dotnetRef, id, tasks) {
        return new class {
            constructor(id, tasks) {
                this.gantt = new Gantt(id, tasks, {
                    view_modes: ["Quarter Day", "Half Day", "Day", "Week", "Month"],
                    view_mode: "Day",
                    date_format: "YYYY-MM-DD",
                    on_click: function (task) {
                        dotnetRef?.invokeMethodAsync('OnClick', task);
                    },
                    on_date_change: function (task, start, end) {
                        dotnetRef?.invokeMethodAsync('OnDateChange', task, start, end);
                    },
                    on_progress_change: function (task, progress) {
                        dotnetRef?.invokeMethodAsync('OnProgressChange', task, progress);
                    },
                    on_view_change: function (mode) {
                        dotnetRef?.invokeMethodAsync('OnViewChange', mode);
                    },
                });
            }

            setDataSource(tasks) {
                this.gantt.refresh(tasks);
            }
            
            setViewMode(mode){
                this.gantt.change_view_mode(mode);
            }
        }(id, tasks);
    }

    window.FrappeGanttModule = {create};
})();