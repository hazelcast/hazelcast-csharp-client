package com.hazelcast.enterprise;

public class License {

    public final boolean full;
    public final boolean enterprise;
    public final int day;
    public final int month;
    public final int year;
    public final int nodes;

    License(final boolean full, final boolean enterprise,
            final int day, final int month, final int year, final int nodes) {
        this.full = full;
        this.enterprise = enterprise;
        this.day = day;
        this.month = month;
        this.year = year;
        this.nodes = nodes;
    }

    @Override
    public boolean equals(final Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;

        final License license = (License) o;

        if (day != license.day) return false;
        if (enterprise != license.enterprise) return false;
        if (full != license.full) return false;
        if (month != license.month) return false;
        if (nodes != license.nodes) return false;
        if (year != license.year) return false;

        return true;
    }

    @Override
    public int hashCode() {
        int result = (full ? 1 : 0);
        result = 31 * result + (enterprise ? 1 : 0);
        result = 31 * result + day;
        result = 31 * result + month;
        result = 31 * result + year;
        result = 31 * result + nodes;
        return result;
    }

    @Override
    public String toString() {
        final StringBuilder sb = new StringBuilder();
        sb.append("License");
        sb.append("{full=").append(full);
        sb.append(", enterprise=").append(enterprise);
        sb.append(", day=").append(day);
        sb.append(", month=").append(month);
        sb.append(", year=").append(year);
        sb.append(", nodes=").append(nodes);
        sb.append('}');
        return sb.toString();
    }
}
